// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.Tools;

public interface IBackstageToolsExecutor : IBackstageService
{
    void Start( BackstageTool tool, string arguments );
}

public interface IBackstageToolsLocator : IBackstageService
{
    bool ToolsMustBeExtracted { get; }

    string GetToolDirectory( BackstageTool tool );
}