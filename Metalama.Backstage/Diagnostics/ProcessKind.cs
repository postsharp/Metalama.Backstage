// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;

namespace Metalama.Backstage.Diagnostics;

public enum ProcessKind
{
    /// <summary>
    /// Not a logged process (used for tests).
    /// </summary>
    /// 
    Other,
    Compiler,
    DevEnv,
    RoslynCodeAnalysisService,
    Rider,
    BackstageWorker,

    [PublicAPI]
    DotNetTool,

    [PublicAPI]
    TestHost,
    OmniSharp,
    CodeLensService
}