// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
    MetalamaConfig
}