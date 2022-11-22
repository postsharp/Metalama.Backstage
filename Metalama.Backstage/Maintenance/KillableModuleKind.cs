// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Maintenance;

[Flags]
internal enum KillableModuleKind
{
    None,
    StandaloneProcess = 1,
    DotNet = 2,
    Both = 3
}