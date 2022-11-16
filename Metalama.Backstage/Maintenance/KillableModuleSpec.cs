namespace Metalama.Backstage;

internal readonly record struct KillableModuleSpec( string Name, KillableModuleKind Kind, string? DisplayName = null )
{
    public bool IsDotNet => (this.Kind & KillableModuleKind.DotNet) != 0;

    public bool IsStandaloneProcess => (this.Kind & KillableModuleKind.DotNet) != 0;
}