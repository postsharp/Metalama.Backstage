using System;

namespace Metalama.Backstage;

[Flags]
internal enum KillableModuleKind
{
    None,
    StandaloneProcess = 1,
    DotNet = 2,
    Both = 3
}