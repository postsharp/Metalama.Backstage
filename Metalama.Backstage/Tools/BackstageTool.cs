// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Tools;

public sealed class BackstageTool
{
    private BackstageTool( string name )
    {
        this.Name = name;
    }

    public static BackstageTool Worker { get; } = new( "Metalama.Backstage.Worker" );

    public static BackstageTool DesktopWindows { get; } = new( "Metalama.Backstage.Desktop.Windows" );

    public string Name { get; }

    public override string ToString() => this.Name;
}