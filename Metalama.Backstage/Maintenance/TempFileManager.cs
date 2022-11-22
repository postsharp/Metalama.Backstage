// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace Metalama.Backstage.Maintenance;

public class TempFileManager : ITempFileManager
{
    private readonly CleanUpConfiguration _configuration;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;
    private readonly IStandardDirectories _standardDirectories;
    private readonly IDateTimeProvider _time;
    private readonly IConfigurationManager _configurationManager;
    private readonly string _version;

    public TempFileManager( IServiceProvider serviceProvider )
    {
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._configuration = this._configurationManager.Get<CleanUpConfiguration>();
        this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "TempFileManager" );
        this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._standardDirectories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();

        var application = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;

        this._version = application.GetLatestComponentMadeByPostSharp().Version ??
                        throw new InvalidOperationException( "The application version is not set." );
    }

    /// <summary>
    /// Cleans Metalama cache subdirectories, conforming the policies set in <c>cleanup.json</c> for each directory. This method gets automatically called from compiler as well.
    /// </summary>
    /// <param name="force">Ignore last clean-up time.</param>
    public void CleanTempDirectories( bool force = false, bool all = false )
    {
        if ( !MutexHelper.WithGlobalLock( "CleanUp", TimeSpan.FromMilliseconds( 1 ), out var mutex ) )
        {
            this._logger.Warning?.Log( "Clean-up is already running." );

            return;
        }

        this._logger.Info?.Log( "Cleaning Metalama temporary files." );

        try
        {
            var now = this._time.Now;
            var lastCleanUpTime = this._configuration.LastCleanUpTime;
            var nextCleanUpTime = lastCleanUpTime?.AddDays( 1 );

            if ( !force &&
                 lastCleanUpTime != null &&
                 now < nextCleanUpTime )
            {
                this._logger.Info?.Log(
                    $"It's not time to clean up cache directories yet. Next clean-up time: {nextCleanUpTime}. Use --force to override this condition." );

                return;
            }

            // Go through all cache directories in temp directory (i.e. CrashReports, ExtractExceptions, Logs etc.)
            this.DeleteDirectoryRecursive( this._standardDirectories.TempDirectory, all );
        }
        finally
        {
            mutex.Dispose();
            this._configurationManager.Update<CleanUpConfiguration>( c => c with { LastCleanUpTime = this._time.Now } );
        }
    }

    private void DeleteDirectoryRecursive( string directory, bool all )
    {
        // Delete the directory if it contains cleanup.json and the cleanup policy requires it.
        if ( this.MustDeleteDirectory( directory, all ) )
        {
            this.DeleteDirectory( directory );
        }
        else
        {
            // Process subdirectories. There are subdirectories after the previous step when the directory does not contain cleanup.json.
            foreach ( var subdirectory in this._fileSystem.EnumerateDirectories( directory ) )
            {
                this.DeleteDirectoryRecursive( subdirectory, all );
            }

            // If the directory became empty, we delete it.
            if ( this._fileSystem.IsDirectoryEmpty( directory ) )
            {
                this.DeleteDirectory( directory );
            }
        }
    }

    private bool MustDeleteDirectory( string directory, bool all )
    {
        var cleanUpFilePath = Path.Combine( directory, "cleanup.json" );

        // If we find the cleanup file in directory, the directory will be deleted.
        if ( !this._fileSystem.FileExists( cleanUpFilePath ) )
        {
            if ( !all )
            {
                return false;
            }
            else
            {
                // When we delete all files, we only delete leave directories, so we delete them depth-first.
                return !this._fileSystem.GetDirectories( directory ).Any();
            }
        }

        if ( all )
        {
            return true;
        }
        else
        {
            var jsonFileContent = this._fileSystem.ReadAllText( cleanUpFilePath );

            CleanUpFile? cleanUpFile;

            try
            {
                cleanUpFile = JsonConvert.DeserializeObject<CleanUpFile>( jsonFileContent );
            }
            catch ( JsonException e )
            {
                this._logger.Error?.Log( $"Cannot deserialize '{jsonFileContent}': {e.Message}" );

                return false;
            }

            if ( cleanUpFile != null )
            {
                var lastWriteTime = this._fileSystem.GetFileLastWriteTime( cleanUpFilePath );

                if ( cleanUpFile.Strategy == CleanUpStrategy.Always
                     || (cleanUpFile.Strategy == CleanUpStrategy.WhenUnused && lastWriteTime < DateTime.Now.AddDays( -7 )) )
                {
                    return true;
                }
                else
                {
                    this._logger.Trace?.Log( $"The directory '{directory}' has been recently used and will not be deleted unless you use the --all option." );

                    return false;
                }
            }
            else
            {
                this._logger.Error?.Log( $"Cannot deserialize '{jsonFileContent}': the file is empty." );

                return false;
            }
        }
    }

    private bool DeleteDirectory( string directory )
    {
        this._logger.Trace?.Log( $"Deleting '{directory}'." );

        for ( var i = 0; i < 100; i++ )
        {
            var newName = directory + i;

            if ( !this._fileSystem.DirectoryExists( newName ) )
            {
                try
                {
                    this._fileSystem.MoveDirectory( directory, newName );
                    this._fileSystem.DeleteDirectory( newName, true );
                }
                catch ( Exception e )
                {
                    this._logger.Warning?.Log( $"Cannot delete '{directory}': {e.Message}" );

                    return false;
                }

                return true;
            }
        }

        this._logger.Warning?.Log(
            $"Directory '{directory}' could not be renamed, this is likely caused by another directory with same name exists in the same location." );

        return false;
    }

    public string GetTempDirectory( string subdirectory, CleanUpStrategy cleanUpStrategy, Guid? guid )
    {
        var directory = Path.Combine(
            this._standardDirectories.TempDirectory,
            subdirectory,
            this._version,
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
                RetryHelper.Retry( () => File.SetLastWriteTime( cleanUpFilePath, DateTime.Now ) );
            }
        }

        return directory;
    }
}