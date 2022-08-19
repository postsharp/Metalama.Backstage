// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;

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

    public void CleanDirectories()
    {
        // TODO: Rename c to cacheDirectory.
        foreach ( var c in Directory.EnumerateDirectories( this._standardDirectories.TempDirectory ) )
        {
            var cacheDirectory = @"C:\Users\JanHlavac\AppData\Local\Temp\Metalama\potato - Copy";
            Console.WriteLine( $"Cleaning {cacheDirectory}" );

            foreach ( var cleanUpFilePath in Directory.EnumerateFiles( cacheDirectory, "cleanup.json", SearchOption.AllDirectories ) )
            {
                Console.WriteLine(  );
                Console.WriteLine( $" - Cleanup file: {cleanUpFilePath}" );
                var jsonContents = File.ReadAllText( cleanUpFilePath );
                Console.WriteLine( jsonContents );
                var cleanUpFile = JsonConvert.DeserializeObject<CleanUpFile>( jsonContents );
                var lastWriteTime = File.GetLastWriteTime( cleanUpFilePath );
                Console.WriteLine( $"   - Strategy: {cleanUpFile.Strategy}" );
                Console.WriteLine( $"   - LastWriteTime: {lastWriteTime}" );
                
                var directoryToDeleteInfo = Directory.GetParent( cleanUpFilePath );
                var directoryToDeletePath = string.Empty;

                if ( directoryToDeleteInfo != null )
                {
                    directoryToDeletePath = directoryToDeleteInfo.FullName;
                }

                Console.WriteLine( $"   - Directory to delete: {directoryToDeletePath}" );

                // WhenUnused
                if ( cleanUpFile.Strategy == CleanUpStrategy.WhenUnused )
                {
                    if ( lastWriteTime < DateTime.Now.AddDays( -7 ) )
                    {
                        Console.WriteLine( "   - Delete, File is older than 7 days." );

                        if ( !string.IsNullOrEmpty( directoryToDeletePath ) )
                        {
                            var directoryToDelete = RenameDirectory( directoryToDeletePath );
                            DeleteDirectory( directoryToDelete );

                            var parentDirectory = Directory.GetParent( directoryToDelete );

                            if ( parentDirectory != null )
                            {
                                var parentDirectoryPath = parentDirectory.FullName;

                                if ( parentDirectoryPath != cacheDirectory )
                                {
                                    DeleteDirectory( parentDirectoryPath );
                                } 
                            }
                        }

                        // TODO: Remove later.
                        continue;

                        // Delete
                    }

                    Console.WriteLine( "   - Not older than 7 days, won't delete." );
                }

                // Always
                else
                {
                    Console.WriteLine( "   - Delete always" );

                    var directoryToDelete = RenameDirectory( directoryToDeletePath );
                    DeleteDirectory( directoryToDelete );

                    var parentDirectory = Directory.GetParent( directoryToDelete );

                    if ( parentDirectory != null )
                    {
                        var parentDirectoryPath = parentDirectory.FullName;

                        if ( parentDirectoryPath != cacheDirectory )
                        {
                            DeleteDirectory( parentDirectoryPath );
                        } 
                    }

                    // Delete
                }
            }

            break;
        }
    }

    public static string RenameDirectory( string directory )
    {
        var newDirectoryName = directory + "_to_delete";

        // TODO: Remove print.
        Console.WriteLine( $"Renaming {directory} to {newDirectoryName}" );
        Directory.Move( directory, newDirectoryName );
        
        return newDirectoryName;
    }

    public void DeleteDirectory( string directory )
    {
        // TODO: Remove print.;
        Console.WriteLine( $"Deleting {directory}." );

        try
        {
            Directory.Delete( directory, true );
        }
        catch ( Exception e )
        {
            this._logger.Warning?.Log( e.Message );
        }
    }

    public void DeleteFile( string file )
    {
        // TODO: Remove print.
        Console.WriteLine( $"Deleting {file}." );

        try
        {
            File.Delete( file );
        }
        catch ( Exception e )
        {
            this._logger.Warning?.Log( e.Message );
        }
    }

    public void CleanAllDirectoriesIgnoringCleanUpPolicies()
    {
        foreach ( var cacheDirectory in Directory.EnumerateDirectories( this._standardDirectories.TempDirectory ) )
        {
            Console.WriteLine( $"Cleaning {cacheDirectory}." );

            foreach ( var subdirectory in Directory.EnumerateDirectories( cacheDirectory ) )
            {
                this.DeleteDirectory( subdirectory );
            }

            foreach ( var file in Directory.EnumerateFiles( cacheDirectory ) )
            {
                this.DeleteFile( file );
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