// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Maintenance;

public class CleanUpTests : TestsBase
{
    private readonly int _subdirectoriesPerCacheDirectory = 5;
    private readonly IStandardDirectories _standardDirectories;

    private readonly ImmutableDictionary<string, CleanUpStrategy> _cleanUpStrategyByDirectory = new Dictionary<string, CleanUpStrategy>()
    {
        { "AssemblyLocator", CleanUpStrategy.WhenUnused },
        { "CompileTime", CleanUpStrategy.WhenUnused },
        { "CompileTimeTroubleshooting", CleanUpStrategy.Always },
        { "CrashReports", CleanUpStrategy.Always },
        { "Extract", CleanUpStrategy.None },
        { "Logs", CleanUpStrategy.Always },
        { "Preview", CleanUpStrategy.WhenUnused },
        { "Tests", CleanUpStrategy.WhenUnused }
    }.ToImmutableDictionary();

    public CleanUpTests( ITestOutputHelper logger ) : base(
        logger,
        builder =>
        {
            builder
                .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() ) )
                .AddConfigurationManager();
        } )
    {
        this._standardDirectories = this.ServiceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this.SetupTempDirectory();
    }

    private void SetupTempDirectory()
    {
        // Create main directories.
        this.FileSystem.CreateDirectory( this._standardDirectories.ApplicationDataDirectory );
        var tempDirectoryPath = this._standardDirectories.TempDirectory;
        this.FileSystem.CreateDirectory( tempDirectoryPath );

        // Populate the cache directories with cleanup files according to strategies.
        foreach ( var pair in this._cleanUpStrategyByDirectory )
        {
            var cacheDirectoryPath = Path.Combine( tempDirectoryPath, pair.Key );

            this.FileSystem.CreateDirectory( cacheDirectoryPath );
            this.Logger.WriteLine( $"Populating '{cacheDirectoryPath}'." );

            for ( var i = 0; i < this._subdirectoriesPerCacheDirectory; i++ )
            {
                var subdirectoryPath = Path.Combine( cacheDirectoryPath, $"0.1.{42 + i}-test" );
                this.FileSystem.CreateDirectory( subdirectoryPath );

                var subdirectoryCleanUpFilePath = Path.Combine( subdirectoryPath, "cleanup.json" );
                var cacheDirectoryStrategy = pair.Value;
                var cleanUpFile = new CleanUpFile { Strategy = cacheDirectoryStrategy };
                var cleanUpStrategy = JsonConvert.SerializeObject( cleanUpFile );
                this.FileSystem.WriteAllText( subdirectoryCleanUpFilePath, cleanUpStrategy );
            }
        }

        // Create a deeper directory structure and populate it with cleanup files at the bottom level.
        var rootDirectory = Path.Combine( this._standardDirectories.TempDirectory, "DeepDirectory" );
        this.Logger.WriteLine( $"Populating '{rootDirectory}' with deeper structure of directories." );

        for ( var i = 0; i < 5; i++ )
        {
            var currentDirectory = Path.Combine( rootDirectory, $"subdirectory_{i}" );

            for ( var j = 0; j < 4; j++ )
            {
                currentDirectory = Path.Combine( currentDirectory, $"subdirectory{i}{j}" );
            }

            this.FileSystem.CreateDirectory( currentDirectory );
            var cleanUpFile = new CleanUpFile { Strategy = CleanUpStrategy.Always };
            var cleanUpStrategy = JsonConvert.SerializeObject( cleanUpFile );
            this.FileSystem.WriteAllText( Path.Combine( currentDirectory, "cleanup.json" ), cleanUpStrategy );
        }
    }

    /// <summary>
    /// Looks for every cleanup.json file recursively in specified <paramref name="path"/> and sets specified <paramref name="lastWriteTime"/> of the file.
    /// </summary>
    /// <param name="path">Path from where you look for the clean-up files.</param>
    /// <param name="lastWriteTime"></param>
    private void SetLastWriteTimeOfCleanUpFile( string path, DateTime lastWriteTime )
    {
        var cleanUpFilePaths = this.FileSystem.EnumerateFiles( path, "cleanup.json", SearchOption.AllDirectories );

        foreach ( var cleanUpFile in cleanUpFilePaths )
        {
            this.FileSystem.SetFileLastWriteTime( cleanUpFile, lastWriteTime );
        }
    }

    [Fact]
    public void Clean_All()
    {
        // Clean-up command should clean everything.
        var tempFileManager = new TempFileManager( this.ServiceProvider );
        tempFileManager.CleanTempDirectories( true, true );

        // Assert every cache directory is deleted.
        Assert.False( this.FileSystem.DirectoryExists( this._standardDirectories.TempDirectory ) );
    }

    [Fact]
    public void Clean_NoOutdatedFiles()
    {
        // Make all cleanup.json files with recent last write time.
        this.SetLastWriteTimeOfCleanUpFile( this._standardDirectories.TempDirectory, DateTime.Now );

        // Clean-up command should skip directories with WhenUnused, as those files are recently created.
        var tempFileManager = new TempFileManager( this.ServiceProvider );
        tempFileManager.CleanTempDirectories( true );

        foreach ( var cacheDirectory in this._cleanUpStrategyByDirectory )
        {
            var cacheDirectoryPath = Path.Combine( this._standardDirectories.TempDirectory, cacheDirectory.Key );

            switch ( cacheDirectory.Value )
            {
                case CleanUpStrategy.WhenUnused:
                    // Assert there are still uncleaned directories as they have been used in past 7 days.
                    Assert.True( this.FileSystem.DirectoryExists( cacheDirectoryPath ) );
                    Assert.NotEmpty( this.FileSystem.EnumerateDirectories( cacheDirectoryPath ) );

                    break;

                case CleanUpStrategy.Always:
                    // Assert always cleaned directories are empty.
                    Assert.False( this.FileSystem.DirectoryExists( cacheDirectoryPath ) );

                    break;

                default:
                    // No strategy directories should be kept.
                    Assert.NotEmpty( this.FileSystem.EnumerateDirectories( cacheDirectoryPath ) );

                    break;
            }
        }
    }

    [Fact]
    public void Clean_OutdatedFiles()
    {
        // Make all cleanup files outdated.
        this.SetLastWriteTimeOfCleanUpFile( this._standardDirectories.TempDirectory, DateTime.Now.AddDays( -10 ) );

        // Clean-up command should clean directories with WhenUnused, as those files are outdated.
        var tempFileManager = new TempFileManager( this.ServiceProvider );
        tempFileManager.CleanTempDirectories( true );

        foreach ( var cacheDirectory in this._cleanUpStrategyByDirectory )
        {
            var cacheDirectoryPath = Path.Combine( this._standardDirectories.TempDirectory, cacheDirectory.Key );

            switch ( cacheDirectory.Value )
            {
                case CleanUpStrategy.WhenUnused:
                case CleanUpStrategy.Always:
                    // Assert always cleaned directories and outdated directories are empty.
                    Assert.False( this.FileSystem.DirectoryExists( cacheDirectoryPath ) );

                    break;

                // No strategy directories should be kept.
                default:
                    Assert.NotEmpty( this.FileSystem.EnumerateDirectories( cacheDirectoryPath ) );

                    break;
            }
        }
    }

    [Fact]
    public void Clean_Scheduled()
    {
        // Create cleanup.json and update its LastCleanUp property to a week days ago.
        var configurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>();
        configurationManager.Update<CleanUpConfiguration>( c => c with { LastCleanUpTime = DateTime.Now.AddDays( -7 ) } );

        // Clean-up command should begin cleaning as the last clean-up was more than a day ago.
        var tempFileManager = new TempFileManager( this.ServiceProvider );
        tempFileManager.CleanTempDirectories();

        foreach ( var cacheDirectory in this._cleanUpStrategyByDirectory )
        {
            var cacheDirectoryPath = Path.Combine( this._standardDirectories.TempDirectory, cacheDirectory.Key );

            switch ( cacheDirectory.Value )
            {
                case CleanUpStrategy.WhenUnused:
                    // Assert directories no older than 7 days are still not empty.
                    Assert.NotEmpty( this.FileSystem.EnumerateDirectories( cacheDirectoryPath ) );

                    break;

                case CleanUpStrategy.Always:
                    // Assert always cleaned directories are empty.
                    Assert.False( this.FileSystem.DirectoryExists( cacheDirectoryPath ) );

                    break;

                default:
                    // No strategy directories should be kept.
                    Assert.NotEmpty( this.FileSystem.EnumerateDirectories( cacheDirectoryPath ) );

                    break;
            }
        }
    }

    [Fact]
    public void Clean_AttemptedBeforeSchedule()
    {
        // Create cleanup.json with LastCleanUp property that will not start clean.
        var configurationManager = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>();
        configurationManager.Update<CleanUpConfiguration>( c => c with { LastCleanUpTime = DateTime.Now } );

        // Clean-up command should skip cleaning as it was attempted too early.
        var tempFileManager = new TempFileManager( this.ServiceProvider );
        tempFileManager.CleanTempDirectories();

        foreach ( var cacheDirectory in this._cleanUpStrategyByDirectory )
        {
            var cacheDirectoryPath = Path.Combine( this._standardDirectories.TempDirectory, cacheDirectory.Key );

            // Directories should not be cleaned as there last clean-up was less than a day ago.
            var directories = this.FileSystem.EnumerateDirectories( cacheDirectoryPath ).ToList();
            Assert.NotEmpty( directories );
        }
    }

    [Fact]
    public void Clean_DeepDirectoryStructure()
    {
        // Clean-up command should clean be able to clean the deep structured directories.
        var tempFileManager = new TempFileManager( this.ServiceProvider );
        tempFileManager.CleanTempDirectories( true );

        // Assert cleanup leaves no leftover cleanup files in deep directory structure.
        Assert.False( this.FileSystem.DirectoryExists( Path.Combine( this._standardDirectories.TempDirectory, "DeepDirectory" ) ) );
    }
}