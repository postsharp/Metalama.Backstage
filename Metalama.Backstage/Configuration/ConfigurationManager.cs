// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Metalama.Backstage.Configuration
{
    internal sealed class ConfigurationManager : IConfigurationManager
    {
        private static readonly TimeSpan _lastModifiedTolerance = TimeSpan.FromSeconds( 0.2 );

        // Stores the in-memory configuration object. Note that ConfigurationFile can be implemented in a different assembly, and that
        // there may be several copies of this assembly in the current AppDomain. Therefore, this dictionary may contain several objects
        // that represent the same file.
        private readonly ConcurrentDictionary<Type, ConfigurationFile> _instances = new();

        private readonly FileSystemWatcher? _fileSystemWatcher;
        private readonly IFileSystem _fileSystem;
        private readonly IEnvironmentVariableProvider _environmentVariableProvider;

        // Named semaphore to handle many instances.
        private readonly Mutex _mutex = new( false, "Global\\Metalama.Configuration" );

        public ConfigurationManager( IServiceProvider serviceProvider )
        {
            var applicationInfo = serviceProvider.GetBackstageService<IApplicationInfoProvider>()?.CurrentApplication;
            this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
            this._environmentVariableProvider = serviceProvider.GetRequiredBackstageService<IEnvironmentVariableProvider>();
            this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "Configuration" );

            this.ApplicationDataDirectory = serviceProvider.GetRequiredBackstageService<IStandardDirectories>().ApplicationDataDirectory;

            if ( !this._fileSystem.DirectoryExists( this.ApplicationDataDirectory ) )
            {
                this._fileSystem.CreateDirectory( this.ApplicationDataDirectory );
            }

            if ( applicationInfo is { IsLongRunningProcess: true } )
            {
                this._fileSystemWatcher = new FileSystemWatcher( this.ApplicationDataDirectory, "*.json" );
                this._fileSystemWatcher.Created += this.OnFileChanged;
                this._fileSystemWatcher.Changed += this.OnFileChanged;
                this._fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnFileChanged( object sender, FileSystemEventArgs e )
        {
            this.Logger.Trace?.Log( $"File has changed: '{e.FullPath}'." );
            var fileName = e.FullPath;

            var existingFiles = this._instances.Values.Where(
                s =>
                    string.Equals( this.GetFileName( s.GetType() ), fileName, StringComparison.OrdinalIgnoreCase ) );

            foreach ( var existingFile in existingFiles )
            {
                // To frequent avoid file locks, wait. There is another wait cycle in TryLoadSettings but not
                // waiting here is annoying for debugging.
                Thread.Sleep( 100 );

                if ( this.TryLoadConfigurationFile( existingFile.GetType(), out var newSettings, out _ ) &&
                     newSettings.LastModified > existingFile.LastModified + _lastModifiedTolerance )
                {
                    existingFile.CopyFrom( newSettings );
                }
            }
        }

        public string ApplicationDataDirectory { get; }

        public ILogger Logger { get; }

        public string GetFileName( Type type )
        {
            var attribute = type.GetCustomAttribute<ConfigurationFileAttribute>();

            if ( attribute == null )
            {
                throw new InvalidOperationException( $"'{nameof(ConfigurationFileAttribute)}' custom attribute not found for '{type.FullName}' type." );
            }

            return Path.Combine( this.ApplicationDataDirectory, attribute.FileName );
        }

        public ConfigurationFile Get( Type type, bool ignoreCache = false )
        {
            ConfigurationFile GetCore()
            {
                this.Logger.Trace?.Log( $"Loading configuration {type.Name} from file." );

                if ( this.TryLoadConfigurationFile( type, out var value, out var fileName ) )
                {
                    return value;
                }

                var settingsObject = Activator.CreateInstance( type );

                if ( settingsObject == null )
                {
                    throw new InvalidOperationException( $"Failed to create instance of '{type.FullName}' type." );
                }

                var settings = (ConfigurationFile) settingsObject;
                settings.Initialize( this, fileName, null );

                return settings;
            }

            // Diagnostics configuration set by process environment variable always overrides local configuration, if it exists.
            if ( type == typeof(DiagnosticsConfiguration) )
            {
                if ( !string.IsNullOrEmpty( this._environmentVariableProvider.GetEnvironmentVariable( "METALAMA_DIAGNOSTICS" ) ) )
                {
                    var environmentDiagnosticsConfiguration = this._environmentVariableProvider.GetDiagnosticsConfigurationFromEnvironmentVariable(
                        "METALAMA_DIAGNOSTICS" );

                    if ( environmentDiagnosticsConfiguration != null )
                    {
                        return environmentDiagnosticsConfiguration;
                    }
                }
            }

            ConfigurationFile settings;
            
            if ( ignoreCache )
            {
                settings = GetCore();

                if ( this._instances.TryGetValue( type, out var existing ) )
                {
                    existing.CopyFrom( settings );
                }
                else
                {
                    this._instances.TryAdd( type, settings );
                }
            }
            else
            {
                settings = this._instances.GetOrAdd( type, _ => GetCore() );
            }

            if ( type == typeof(DiagnosticsConfiguration) )
            {
                var diagnostics = (DiagnosticsConfiguration) settings;

                if ( diagnostics.LastModified < DateTime.Now.AddHours( diagnostics.Logging.StopLoggingAfterHours * -1 ) )
                {
                    diagnostics.DisableLogging();

                    return diagnostics;
                }
            }
            
            return settings;
        }

        public bool TryUpdate( ConfigurationFile value, DateTime? lastModified )
        {
            this._mutex.WaitOne();

            try
            {
                var fileName = this.GetFileName( value.GetType() );

                this.Logger.Trace?.Log( $"Trying to update '{fileName}'. Our last timestamp is '{lastModified}'." );

                // Verify (inside the global lock) that we have a fresh copy of the file.
                if ( lastModified == null )
                {
                    if ( this._fileSystem.FileExists( fileName ) )
                    {
                        this.Logger.Warning?.Log( $"Cannot update '{fileName}' because the file exists but it was not supposed to." );

                        return false;
                    }
                }
                else if ( !this._fileSystem.FileExists( fileName ) || this._fileSystem.GetLastWriteTime( fileName ) != lastModified )
                {
                    this.Logger.Warning?.Log( $"Cannot update '{fileName}' because the file did not exists or had a different timestamp." );

                    return false;
                }

                if ( this._instances.TryGetValue( value.GetType(), out var baseValue ) )
                {
                    if ( lastModified.HasValue && baseValue.LastModified != lastModified.Value )
                    {
                        this.Logger.Warning?.Log( $"Cannot update '{fileName}' because our cached copy has an invalid timestamp." );

                        return false;
                    }
                    else
                    {
                        baseValue.CopyFrom( value );
                    }
                }
                else
                {
                    baseValue = value.Clone();

                    if ( !this._instances.TryAdd( value.GetType(), baseValue ) )
                    {
                        this.Logger.Error?.Log( $"Cannot update '{fileName}': TryAdd returned false." );

                        return false;
                    }
                }

                var json = value.ToJson();

                // We have to wait more time than the time resolution of DateTime or the file system.
                Thread.Sleep( 1 );

                RetryHelper.Retry( () => this._fileSystem.WriteAllText( fileName, json ) );

                // Intentionally update the timestamp a second time. 
                baseValue.LastModified = this._fileSystem.GetLastWriteTime( fileName );

                this.Logger.Trace?.Log( $"File '{fileName}' updated. The new timestamp is '{baseValue.LastModified}'." );
            }
            finally
            {
                this._mutex.ReleaseMutex();
            }

            return true;
        }

        private bool TryLoadConfigurationFile( Type type, [NotNullWhen( true )] out ConfigurationFile? settings, out string fileName )
        {
            fileName = this.GetFileName( type );

            if ( !this._fileSystem.FileExists( fileName ) )
            {
                this.Logger.Trace?.Log( $"The file '{fileName}' does not exist." );

                settings = null;

                return false;
            }

            try
            {
                var fileNameCopy = fileName;

                this.Logger.Trace?.Log( $"Reading configuration file '{fileName}'." );

                var json = RetryHelper.Retry( () => this._fileSystem.ReadAllText( fileNameCopy ) );

                settings = (ConfigurationFile?) JsonConvert.DeserializeObject( json, type );

                if ( settings == null )
                {
                    return false;
                }

                settings.Initialize( this, fileName, this._fileSystem.GetLastWriteTime( fileName ) );

                return true;
            }
            catch ( Exception e )
            {
                this.Logger.Error?.Log( $"Error reading file '{fileName}': " + e );
            }

            settings = default;

            return false;
        }

        public void Dispose()
        {
            this._mutex.Dispose();
            this._fileSystemWatcher?.Dispose();
        }
    }
}