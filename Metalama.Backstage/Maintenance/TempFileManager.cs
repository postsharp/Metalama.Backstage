// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace Metalama.Backstage.Maintenance;

public class TempFileManager : ITempFileManager
{
    private readonly CleanUpConfiguration _configuration;
    private readonly IApplicationInfoProvider _applicationInfoProvider;
    private readonly ILogger _logger;
    private readonly IStandardDirectories _standardDirectories;
    private readonly IDateTimeProvider _time;

    public TempFileManager( IServiceProvider serviceProvider )
    {
        var configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._configuration = configurationManager.Get<CleanUpConfiguration>();
        this._applicationInfoProvider = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>();
        this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._standardDirectories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this._logger = serviceProvider.GetLoggerFactory().Telemetry();
    }

    public static bool IsDirectoryEmpty( string path ) => !Directory.EnumerateFileSystemEntries( path ).Any();

    /// <summary>
    /// Cleans everything in Metalama cache ignoring clean-up policies.
    /// </summary>
    public void CleanEverythingIgnoringCleanUpPolicies()
    {
        if ( !MutexHelper.WithGlobalLock( "CleanUp", TimeSpan.FromMilliseconds( 1 ), out var mutex ) )
        {
            this._logger.Info?.Log( "Clean-up is already running." );

            return;
        }
        
        try
        {
            foreach ( var cacheDirectory in Directory.EnumerateDirectories( this._standardDirectories.TempDirectory ) )
            {
                this._logger.Info?.Log( $"Cleaning inside '{cacheDirectory}'." );

                this.DeleteAllSubdirectories( cacheDirectory );
                this.DeleteAllFilesInDirectory( cacheDirectory );
            }
        }
        catch ( UnauthorizedAccessException e )
        {
            this._logger.Warning?.Log( e.Message );
        }
        finally
        {
            this._configuration.ConfigurationManager.Update<CleanUpConfiguration>( c => c.ResetLastCleanUpTime() );
            mutex.Dispose();
        }
    }

    /// <summary>
    /// Cleans Metalama cache subdirectories, conforming the policies set in <c>cleanup.json</c> for each directory. This method gets automatically called from compiler as well.
    /// </summary>
    /// <param name="force">Ignore last clean-up time.</param>
    public void CleanDirectoriesRespectingCleanupPolicies( bool force = false )
    {
        if ( !MutexHelper.WithGlobalLock( "CleanUp", TimeSpan.FromMilliseconds( 1 ), out var mutex ) )
        {
            this._logger.Info?.Log( "Clean-up is already running." );

            return;
        }

        var now = this._time.Now;
        var lastCleanUpTime = this._configuration.LastCleanUpTime;

        if ( !force &&
             lastCleanUpTime != null &&
             lastCleanUpTime.Value.AddDays( 1 ) >= now )
        {
            this._logger.Info?.Log( $"It's not time to clean up cache directories yet. Now: {now} Last clean-up time: {lastCleanUpTime}" );

            return;
        }

        // Process all directories in temp directory.
        foreach ( var cacheDirectory in Directory.EnumerateDirectories( this._standardDirectories.TempDirectory ) )
        {
            try
            {
                this._logger.Info?.Log( $"Starting clean-up of '{cacheDirectory}' directory." );

                // Look for all cleanup.json files to determine what directories need to be cleaned.
                foreach ( var cleanUpFilePath in Directory.EnumerateFiles( cacheDirectory, "cleanup.json", SearchOption.AllDirectories ) )
                {
                    var jsonFileContent = File.ReadAllText( cleanUpFilePath );
                    var cleanUpFile = JsonConvert.DeserializeObject<CleanUpFile>( jsonFileContent );

                    if ( cleanUpFile == null )
                    {
                        this._logger.Warning?.Log( $"File '{cleanUpFilePath}' is empty." );

                        continue;
                    }

                    var lastWriteTime = File.GetLastWriteTime( cleanUpFilePath );

                    var directoryToCleanUpInfo = Directory.GetParent( cleanUpFilePath );
                    var directoryToCleanUpPath = string.Empty;

                    if ( directoryToCleanUpInfo != null )
                    {
                        directoryToCleanUpPath = directoryToCleanUpInfo.FullName;
                    }

                    this._logger.Info?.Log( $"Cleaning '{directoryToCleanUpPath}'." );

                    // Always clean or clean only if the cleanup file has been modified more than 7 days ago.
                    if ( cleanUpFile.Strategy == CleanUpStrategy.Always
                         || (cleanUpFile.Strategy == CleanUpStrategy.WhenUnused && lastWriteTime < DateTime.Now.AddDays( -7 )) )
                    {
                        this.DeleteAllSubdirectories( directoryToCleanUpPath );
                        this.DeleteAllFilesInDirectory( directoryToCleanUpPath );
                    }

                    // Empty parent directory is deleted after cleaning subdirectories and files.
                    if ( IsDirectoryEmpty( directoryToCleanUpPath ) )
                    {
                        var renamedParentDirectory = this.RenameDirectory( directoryToCleanUpPath );
                        this.DeleteDirectory( renamedParentDirectory );
                    }
                }
            }
            catch ( UnauthorizedAccessException e )
            {
                this._logger.Warning?.Log( e.Message );
            }
        }

        mutex.Dispose();
        this._configuration.ConfigurationManager.Update<CleanUpConfiguration>( c => c.ResetLastCleanUpTime() );
    }

    public string RenameDirectory( string directory )
    {
        var newDirectoryName = directory + "_to_delete";

        try
        {
            Directory.Move( directory, newDirectoryName );
        }
        catch ( Exception e )
        {
            this._logger.Warning?.Log( e.Message );
        }

        return newDirectoryName;
    }

    public string RenameFile( string file )
    {
        var fileInfo = new FileInfo( file );
        var newFileName = string.Empty;

        if ( fileInfo.Directory != null )
        {
            newFileName = fileInfo.Directory.FullName + "to_delete" + fileInfo.Extension;
        }

        try
        {
            if ( !string.IsNullOrEmpty( newFileName ) )
            {
                fileInfo.MoveTo( newFileName );
            }
        }
        catch ( Exception e )
        {
            this._logger.Warning?.Log( e.Message );
        }

        return newFileName;
    }

    public void DeleteDirectory( string directoryToDelete )
    {
        try
        {
            Directory.Delete( directoryToDelete, true );
        }
        catch ( Exception e )
        {
            this._logger.Warning?.Log( e.Message );
        }
    }

    /// <summary>
    /// Enumerates all directories in a directory and deletes them.
    /// </summary>
    /// <param name="directory"></param>
    public void DeleteAllSubdirectories( string directory )
    {
        foreach ( var directoryToDelete in Directory.EnumerateDirectories( directory ) )
        {
            this._logger.Info?.Log( $"Deleting '{directoryToDelete}'." );

            var renamedDirectoryToDelete = this.RenameDirectory( directoryToDelete );
            this.DeleteDirectory( renamedDirectoryToDelete );
        }
    }

    /// <summary>
    /// Enumerates all files in a directory and deletes them.
    /// </summary>
    /// <param name="directory"></param>
    public void DeleteAllFilesInDirectory( string directory )
    {
        foreach ( var fileToDelete in Directory.EnumerateFiles( directory ) )
        {
            var renamedFileToDelete = this.RenameFile( fileToDelete );

            this._logger.Info?.Log( $"Deleting '{fileToDelete}'." );

            try
            {
                File.Delete( renamedFileToDelete );
            }
            catch ( Exception e )
            {
                this._logger.Warning?.Log( e.Message );
            }
        }
    }

    public string GetTempDirectory( string subdirectory, CleanUpStrategy cleanUpStrategy, Guid? guid )
    {
        var directory = Path.Combine(
            this._standardDirectories.TempDirectory,
            subdirectory,
            this._applicationInfoProvider.CurrentApplication.Version,
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