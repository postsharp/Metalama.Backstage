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
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEnvironmentVariableProvider _environmentVariableProvider;

        // Named semaphore to handle many instances.
        private readonly Mutex _mutex = new( false, "Global\\Metalama.Configuration" );

        public ConfigurationManager( IServiceProvider serviceProvider )
        {
            var applicationInfo = serviceProvider.GetBackstageService<IApplicationInfoProvider>()?.CurrentApplication;
            this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
            this._dateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
            this._environmentVariableProvider = serviceProvider.GetRequiredBackstageService<IEnvironmentVariableProvider>();
            
            // In a production use, the logger factory is created after the configuration manager, so we will not have any log for
            // this class. However, tests may have their own logging.
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

            var changedSettings = this._instances.Values.Where(
                s =>
                    string.Equals( this.GetFileName( s.GetType() ), fileName, StringComparison.OrdinalIgnoreCase ) );

            using ( this.WithMutex() )
            {
                foreach ( var changedSetting in changedSettings )
                {
                    // To frequent avoid file locks, wait. There is another wait cycle in TryLoadSettings but not
                    // waiting here is annoying for debugging.
                    Thread.Sleep( 100 );

                    if ( this.TryLoadConfigurationFile( changedSetting.GetType(), out var cachedSettings ) &&
                         cachedSettings.LastModified > changedSetting.LastModified + _lastModifiedTolerance )
                    {
                        this.AddToCache( changedSetting );
                    }
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

        private string? GetEnvironmentVariableName( Type type )
        {
            var attribute = type.GetCustomAttribute<ConfigurationFileAttribute>();

            if ( attribute == null )
            {
                throw new InvalidOperationException( $"'{nameof(ConfigurationFileAttribute)}' custom attribute not found for '{type.FullName}' type." );
            }

            return attribute.EnvironmentVariableName;
        }

        public ConfigurationFile Get( Type type, bool ignoreCache = false )
        {
            ConfigurationFile GetCore()
            {
                using ( this.WithMutex() )
                {
                    this.Logger.Trace?.Log( $"Loading configuration {type.Name} from file." );

                    if ( this.TryLoadConfigurationFile( type, out var value ) )
                    {
                        return value;
                    }

                    var settingsObject = Activator.CreateInstance( type );

                    if ( settingsObject == null )
                    {
                        throw new InvalidOperationException( $"Failed to create instance of '{type.FullName}' type." );
                    }

                    var settings = (ConfigurationFile) settingsObject;

                    return settings;
                }
            }

            ConfigurationFile settings;

            if ( ignoreCache )
            {
                settings = GetCore();
                this.AddToCache( settings );

                return settings;
            }
            else
            {
                settings = this._instances.GetOrAdd( type, _ => GetCore() );
            }

            return settings;
        }

        public event Action<ConfigurationFile>? ConfigurationFileChanged;

        private void AddToCache( ConfigurationFile settings )
        {
            this._instances.AddOrUpdate( settings.GetType(), settings, ( _, _ ) => settings );
            this.ConfigurationFileChanged?.Invoke( settings );
        }

        public bool TryUpdate( ConfigurationFile value, DateTime? lastModified )
        {
            using ( this.WithMutex() )
            {
                var type = value.GetType();
                var fileName = this.GetFileName( type );

                this.Logger.Trace?.Log( $"Trying to update '{fileName}'. Our last timestamp is '{lastModified:O}'." );

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

                if ( this._instances.TryGetValue( type, out var originalSettings ) )
                {
                    if ( lastModified.HasValue && originalSettings.LastModified != lastModified.Value )
                    {
                        this.Logger.Warning?.Log( $"Cannot update '{fileName}' because our cached copy does not have the latest timestamp." );

                        return false;
                    }
                }

                var json = value.ToJson();

                RetryHelper.Retry( () => this._fileSystem.WriteAllText( fileName, json ) );

                var newLastModified = this._fileSystem.GetLastWriteTime( fileName );

                while ( newLastModified == lastModified )
                {
                    // The new filesystem timestamp is identical to the previous one, so we have to wait until there is an observable
                    // difference in the timestamp.

                    this.Logger.Trace?.Log( "Waiting for " );
                    Thread.Sleep( 10 );
                    this._fileSystem.SetLastWriteTime( fileName, this._dateTimeProvider.Now );
                    newLastModified = this._fileSystem.GetLastWriteTime( fileName );
                }

                value = value with { LastModified = newLastModified };

                this.AddToCache( value );

                this.Logger.Trace?.Log( $"File '{fileName}' updated. The new timestamp is '{newLastModified:O}'." );
            }

            return true;
        }

        private bool TryLoadConfigurationContent( Type type, string fileName, DateTime lastModified, [NotNullWhen( true )] out string? json )
        {
            // Try to load the json from the environment variable.
            var environmentVariableName = this.GetEnvironmentVariableName( type );
            
            if ( environmentVariableName != null )
            {
                json = this._environmentVariableProvider.GetEnvironmentVariable( environmentVariableName )!;

                if ( !string.IsNullOrWhiteSpace( json ) )
                {
                    this.Logger.Trace?.Log( $"Configuration for {type.Name} loaded from the environment variable '{environmentVariableName}'." );

                    return true;
                }
            }

            // Try to load form the file.
            if ( !this._fileSystem.FileExists( fileName ) )
            {
                this.Logger.Trace?.Log( $"The file '{fileName}' does not exist." );

                json = null;

                return false;
            }

            try
            {
                var fileNameCopy = fileName;

                this.Logger.Trace?.Log( $"Reading configuration file '{fileName}' with timestamp '{lastModified:O}'." );

                json = RetryHelper.Retry( () => this._fileSystem.ReadAllText( fileNameCopy ) );

                return true;
            }
            catch ( Exception e )
            {
                this.Logger.Error?.Log( $"Error reading file '{fileName}': " + e );
            }

            // Could not be loaded.
            json = null;

            return false;
        }

        private bool TryLoadConfigurationFile( Type type, [NotNullWhen( true )] out ConfigurationFile? settings )
        {
            var fileName = this.GetFileName( type );

            var lastModified = this._fileSystem.GetLastWriteTime( fileName );

            if ( !this.TryLoadConfigurationContent( type, fileName, lastModified, out var json ) )
            {
                settings = null;

                return false;
            }

            try
            {
                settings = (ConfigurationFile?) JsonConvert.DeserializeObject( json, type );

                if ( settings == null )
                {
                    return false;
                }

                settings = settings with { LastModified = lastModified };

                return true;
            }
            catch ( Exception e )
            {
                this.Logger.Error?.Log( $"Error reading file '{fileName}': " + e );
            }

            settings = default;

            return false;
        }

        private DisposableAction WithMutex()
        {
            if ( !this._mutex.WaitOne( 0 ) )
            {
                this.Logger.Trace?.Log( $"Waiting for the configuration mutex." );
                this._mutex.WaitOne();
            }

            this.Logger.Trace?.Log( $"Configuration mutex acquired." );

            return new DisposableAction(
                () =>
                {
                    this.Logger.Trace?.Log( $"Releasing configuration mutex." );
                    this._mutex.ReleaseMutex();
                } );
        }

        public void ClearCache()
        {
            this._instances.Clear();
        }

        public void Dispose()
        {
            this._mutex.Dispose();
            this._fileSystemWatcher?.Dispose();
        }
    }
}