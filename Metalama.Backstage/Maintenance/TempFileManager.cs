// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Metalama.Backstage.Maintenance;

public class TempFileManager : ITempFileManager
{
    private readonly CleanUpConfiguration _configuration;
    private readonly IApplicationInfoProvider _applicationInfoProvider;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IStandardDirectories _standardDirectories;
    private readonly IDateTimeProvider _time;

    public TempFileManager( IServiceProvider serviceProvider )
    {
        var configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._configuration = configurationManager.Get<CleanUpConfiguration>();
        this._applicationInfoProvider = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>();
        this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
        this._logger = serviceProvider.GetLoggerFactory().Telemetry();
        this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._standardDirectories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
    }

    /// <summary>
    /// Cleans Metalama cache subdirectories, conforming the policies set in <c>cleanup.json</c> for each directory. This method gets automatically called from compiler as well.
    /// </summary>
    /// <param name="force">Ignore last clean-up time.</param>
    public void CleanTempDirectories( bool force = false, bool all = false )
    {
        if ( !MutexHelper.WithGlobalLock( "CleanUp", TimeSpan.FromMilliseconds( 1 ), out var mutex ) )
        {
            this._logger.Info?.Log( "Clean-up is already running." );

            return;
        }

        try
        {
            var now = this._time.Now;
            var lastCleanUpTime = this._configuration.LastCleanUpTime;

            if ( !force &&
                 lastCleanUpTime != null &&
                 lastCleanUpTime.Value.AddDays( 1 ) >= now )
            {
                this._logger.Info?.Log( $"It's not time to clean up cache directories yet. Now: {now} Last clean-up time: {lastCleanUpTime}" );

                return;
            }

            // Go through all cache directories in temp directory (i.e. CrashReports, ExtractExceptions, Logs etc.)
            foreach ( var cacheDirectory in this._fileSystem.EnumerateDirectories( this._standardDirectories.TempDirectory ) )
            {
                try
                {
                    this._logger.Info?.Log( $"Starting clean-up of '{cacheDirectory}' directory." );

                    // Go through all subdirectories in the cache directory.
                    foreach ( var subdirectory in this._fileSystem.EnumerateDirectories( cacheDirectory ) )
                    {
                        // --all flag will cause the subdirectory to be deleted immediately.
                        if ( all )
                        {
                            var renamedSubdirectory = this.RenameDirectory( subdirectory );
                            this.DeleteDirectory( renamedSubdirectory );

                            continue;
                        }

                        this.DeleteDirectoryRecursive( subdirectory );
                    }
                }
                catch ( Exception e )
                {
                    this._logger.Warning?.Log( e.Message );

                    throw;
                }
            }
        }
        finally
        {
            mutex.Dispose();
            this._configuration.ConfigurationManager.Update<CleanUpConfiguration>( c => c.ResetLastCleanUpTime() );
        }
    }

    public void DeleteDirectoryRecursive( string directory )
    {
        var cleanUpFileCandidate = Path.Combine( directory, "cleanup.json" );

        // If we find the cleanup file in directory, the directory will be deleted.
        if ( this._fileSystem.FileExists( cleanUpFileCandidate ) )
        {
            var jsonFileContent = this._fileSystem.ReadAllText( cleanUpFileCandidate );
            var cleanUpFile = JsonConvert.DeserializeObject<CleanUpFile>( jsonFileContent );

            if ( cleanUpFile != null )
            {
                var lastWriteTime = this._fileSystem.GetLastWriteTime( cleanUpFileCandidate );

                if ( cleanUpFile.Strategy == CleanUpStrategy.Always
                     || (cleanUpFile.Strategy == CleanUpStrategy.WhenUnused && lastWriteTime < DateTime.Now.AddDays( -7 )) )
                {
                    this._logger.Info?.Log( $"Deleting '{directory}'." );

                    var renamedDirectory = this.RenameDirectory( directory );
                    this.DeleteDirectory( renamedDirectory );
                }
            }
        }
        else
        {
            // If no cleanup file is found, we proceed deeper in the directory tree.
            foreach ( var dir in this._fileSystem.GetDirectories( directory ) )
            {
                this.DeleteDirectoryRecursive( dir );
            }

            if ( this._fileSystem.IsDirectoryEmpty( directory ) )
            {
                var renamedDirectory = this.RenameDirectory( directory );
                this.DeleteDirectory( renamedDirectory );
            }
        }
    }

    public string RenameDirectory( string directory )
    {
        for ( var i = 0; i < 100; i++ )
        {
            var newDirectoryName = directory + i;

            if ( !this._fileSystem.DirectoryExists( newDirectoryName ) )
            {
                try
                {
                    this._fileSystem.MoveDirectory( directory, newDirectoryName );
                }
                catch ( Exception e )
                {
                    this._logger.Warning?.Log( e.Message );

                    throw;
                }

                return newDirectoryName;
            }
        }

        throw new InvalidOperationException(
            $"Directory '{directory}' could not be renamed, this is likely caused by another directory with same name exists in the same location." );
    }

    public void DeleteDirectory( string directory )
    {
        try
        {
            this._fileSystem.DeleteDirectory( directory, true );
        }
        catch ( Exception e )
        {
            this._logger.Warning?.Log( e.Message );

            throw;
        }
    }

    public string GetTempDirectory( string subdirectory, CleanUpStrategy cleanUpStrategy, Guid? guid )
    {
        var directory = Path.Combine(
            this._standardDirectories.TempDirectory,
            subdirectory,
            this._applicationInfoProvider.CurrentApplication.Version
            ?? throw new InvalidOperationException( $"Unknown version of '{this._applicationInfoProvider.CurrentApplication.Name}' application." ),
            guid?.ToString() ?? "" );

        var cleanUpFilePath = Path.Combine( directory, "cleanup.json" );

        if ( !Directory.Exists( directory ) || !File.Exists( cleanUpFilePath ) )
        {
            using ( MutexHelper.WithGlobalLock( directory ) )
            {
                if ( !Directory.Exists( directory ) || !File.Exists( cleanUpFilePath ) )
                {
                    if ( !Directory.Exists( directory ) )
                    {
                        Directory.CreateDirectory( directory );
                    }

                    if ( !File.Exists( cleanUpFilePath ) )
                    {
                        var file = new CleanUpFile() { Strategy = cleanUpStrategy };
                        File.WriteAllText( cleanUpFilePath, JsonConvert.SerializeObject( file ) );

                        return directory;
                    }
                }
            }
        }

        if ( cleanUpStrategy == CleanUpStrategy.WhenUnused && File.GetLastAccessTime( cleanUpFilePath ) > DateTime.Now.AddDays( -1 ) )
        {
            using ( MutexHelper.WithGlobalLock( cleanUpFilePath ) )
            {
                File.SetLastWriteTime( cleanUpFilePath, DateTime.Now );
            }
        }

        return directory;
    }
}