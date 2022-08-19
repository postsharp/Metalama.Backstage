// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

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
    private readonly IApplicationInfoProvider _applicationInfoProvider;
    private readonly IStandardDirectories _standardDirectories;
    private readonly ILogger _logger;

    public TempFileManager( IServiceProvider serviceProvider )
    {
        this._standardDirectories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this._applicationInfoProvider = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>();
        this._logger = serviceProvider.GetLoggerFactory().Telemetry();
    }

    public static bool IsDirectoryEmpty( string path ) => !Directory.EnumerateFileSystemEntries( path ).Any();

    public void CleanDirectoriesRespectingCleanupPolicies()
    {
        foreach ( var cacheDirectory in Directory.EnumerateDirectories( this._standardDirectories.TempDirectory ) )
        {
            try
            {
                foreach ( var cleanUpFilePath in Directory.EnumerateFiles( cacheDirectory, "cleanup.json", SearchOption.AllDirectories ) )
                {
                    var jsonContents = File.ReadAllText( cleanUpFilePath );
                    var cleanUpFile = JsonConvert.DeserializeObject<CleanUpFile>( jsonContents );

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

                    switch ( cleanUpFile.Strategy )
                    {
                        case CleanUpStrategy.WhenUnused when lastWriteTime < DateTime.Now.AddDays( -7 ):
                            this.DeleteAllSubdirectories( directoryToCleanUpPath );
                            this.DeleteAllFilesInDirectory( directoryToCleanUpPath );

                            break;

                        case CleanUpStrategy.Always:
                            this.DeleteAllSubdirectories( directoryToCleanUpPath );
                            this.DeleteAllFilesInDirectory( directoryToCleanUpPath );

                            break;
                    }

                    // Delete parent directory unless it's empty.
                    if ( IsDirectoryEmpty( directoryToCleanUpPath ) && directoryToCleanUpPath != cacheDirectory )
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

    public void DeleteAllSubdirectories( string cleanUpDirectory )
    {
        foreach ( var directoryToDelete in Directory.EnumerateDirectories( cleanUpDirectory ) )
        {
            this._logger.Info?.Log( $"Deleting '{directoryToDelete}'." );
            var renamedDirectoryToDelete = this.RenameDirectory( directoryToDelete );
            this.DeleteDirectory( renamedDirectoryToDelete );
        }
    }

    public void DeleteAllFilesInDirectory( string directoryToCleanUpPath )
    {
        foreach ( var fileToDelete in Directory.EnumerateFiles( directoryToCleanUpPath ) )
        {
            this._logger.Info?.Log( $"Deleting '{fileToDelete}'." );
            var renamedFileToDelete = this.RenameFile( fileToDelete );
            
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

    public void CleanAllDirectoriesIgnoringCleanUpPolicies()
    {
        try
        {
            foreach ( var cacheDirectory in Directory.EnumerateDirectories( this._standardDirectories.TempDirectory ) )
            {
                this._logger.Info?.Log( $"Cleaning  '{cacheDirectory}'." );

                this.DeleteAllSubdirectories( cacheDirectory );
                this.DeleteAllFilesInDirectory( cacheDirectory );
            }
        }
        catch ( UnauthorizedAccessException e )
        {
            this._logger.Warning?.Log( e.Message );
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