// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Tools;
using System;
using System.IO;

namespace Metalama.Backstage.Testing;

public class MockBackstageToolLocator : IBackstageToolsLocator
{
    private readonly IFileSystem _fileSystem;

    public MockBackstageToolLocator( IServiceProvider serviceProvider )
    {
        this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
    }

    public bool ToolsMustBeExtracted => false;

    public string GetToolDirectory( BackstageTool tool )
    {
        var fileName = Path.DirectorySeparatorChar + tool.Name;

        // We must simulate that these files exist where they are expected.
        this._fileSystem.CreateDirectory( Path.DirectorySeparatorChar + tool.Name );
        this._fileSystem.WriteAllText( $"{Path.DirectorySeparatorChar}{tool.Name}{Path.DirectorySeparatorChar}{tool.Name}.dll", tool.Name );

        return fileName;
    }
}