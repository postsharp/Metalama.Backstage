// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            var applicationInfo = serviceProvider.GetService<IApplicationInfo>();
            this._fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

            this.ApplicationDataDirectory = serviceProvider.GetRequiredService<IStandardDirectories>().ApplicationDataDirectory;

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

        public string GetFileName( Type type )
        {
            var attribute = type.GetCustomAttribute<ConfigurationFileAttribute>();

            return Path.Combine( this.ApplicationDataDirectory, attribute.FileName );
        }

        public ConfigurationFile Get( Type type, bool ignoreCache = false )
        {
            ConfigurationFile GetCore()
            {
                if ( this.TryLoadConfigurationFile( type, out var value, out var fileName ) )
                {
                    return value;
                }

                var settings = (ConfigurationFile) Activator.CreateInstance( type );
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
                if ( this._instances.TryGetValue( value.GetType(), out var baseValue ) )
                {
                    if ( lastModified.HasValue && baseValue.LastModified != lastModified.Value )
                    {
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
                        return false;
                    }
                }

                var json = value.ToJson();
                var fileName = this.GetFileName( value.GetType() );

                RetryHelper.Retry( () => this._fileSystem.WriteAllText( fileName, json ) );

                // Intentionally update the timestamp a second time. 
                baseValue.LastModified = this._fileSystem.GetLastWriteTime( fileName );
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
                settings = null;

                return false;
            }

            try
            {
                var fileNameCopy = fileName;
                var json = RetryHelper.Retry( () => this._fileSystem.ReadAllText( fileNameCopy ) );

                settings = (ConfigurationFile?) JsonConvert.DeserializeObject( json, type );

                if ( settings == null )
                {
                    return false;
                }

                settings.Initialize( this, fileName, this._fileSystem.GetLastWriteTime( fileName ) );

                return true;
            }
            catch { }

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