// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage;

internal readonly record struct KillableModuleSpec( string Name, KillableModuleKind Kind, string? DisplayName = null )
{
    public bool IsDotNet => (this.Kind & KillableModuleKind.DotNet) != 0;

    public bool IsStandaloneProcess => (this.Kind & KillableModuleKind.DotNet) != 0;
}