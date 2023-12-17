// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Utilities;
using System;
using System.IO;
using System.IO.Compression;

namespace Metalama.Backstage.Tools;

internal abstract class EmbeddedProgram
{
    private readonly IFileSystem _fileSystem;
    private readonly ITempFileManager _tempFileManager;

    protected EmbeddedProgram( IServiceProvider serviceProvider )
    {
        this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
        this._tempFileManager = serviceProvider.GetRequiredBackstageService<ITempFileManager>();
    }

    protected string ExtractProgram( string framework, string programName )
    {
        var directory = this._tempFileManager.GetTempDirectory(
            "Programs",
            subdirectory: Path.Combine( programName, framework ),
            cleanUpStrategy: CleanUpStrategy.WhenUnused );

        var touchFile = Path.Combine( directory, "unzipped.touch" );

        if ( !this._fileSystem.FileExists( touchFile ) )
        {
            using ( MutexHelper.WithGlobalLock( touchFile ) )
            {
                if ( !this._fileSystem.FileExists( touchFile ) )
                {
                    this._fileSystem.CreateDirectory( directory );

                    var zipResourceName = $"Metalama.Backstage.Tools.{programName}.{framework}.zip";
                    var assembly = this.GetType().Assembly;
                    using var resourceStream = assembly.GetManifestResourceStream( zipResourceName );

                    if ( resourceStream == null )
                    {
                        throw new InvalidOperationException( $"Resource '{zipResourceName}' not found in '{assembly.Location}'." );
                    }

                    using var zipStream = new ZipArchive( resourceStream );
                    this._fileSystem.ExtractZipArchiveToDirectory( zipStream, directory );
                    this._fileSystem.WriteAllText( touchFile, "" );
                }
            }
        }

        return directory;
    }
}