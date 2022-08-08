// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

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

        private readonly ConcurrentDictionary<Type, ConfigurationFile> _instances = new();

        private readonly FileSystemWatcher? _fileSystemWatcher;
        private readonly IFileSystem _fileSystem;

        // Named semaphore to handle many instances.
        private readonly Mutex _mutex = new( false, "Global\\Metalama.Configuration" );

        public ConfigurationManager( IServiceProvider serviceProvider )
        {
            var applicationInfo = serviceProvider.GetBackstageService<IApplicationInfoProvider>()?.CurrentApplication;
            this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
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

            var existingSettings = this._instances.Values.SingleOrDefault(
                s =>
                    string.Equals( this.GetFileName( s.GetType() ), fileName, StringComparison.OrdinalIgnoreCase ) );

            if ( existingSettings != null )
            {
                // To frequent avoid file locks, wait. There is another wait cycle in TryLoadSettings but not
                // waiting here is annoying for debugging.
                Thread.Sleep( 100 );

                if ( this.TryLoadConfigurationFile( existingSettings.GetType(), out var newSettings, out _ ) &&
                     newSettings.LastModified > existingSettings.LastModified + _lastModifiedTolerance )
                {
                    existingSettings.CopyFrom( newSettings );
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

            if ( ignoreCache )
            {
                var settings = GetCore();

                if ( this._instances.TryGetValue( type, out var existing ) )
                {
                    existing.CopyFrom( settings );
                }
                else
                {
                    this._instances.TryAdd( type, settings );
                }

                return settings;
            }
            else
            {
                return this._instances.GetOrAdd( type, _ => GetCore() );
            }
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