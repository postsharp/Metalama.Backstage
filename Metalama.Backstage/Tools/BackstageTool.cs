﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Diagnostics;

namespace Metalama.Backstage.Tools;

public sealed class BackstageTool
{
    private BackstageTool( string name, bool isExe, ProcessWindowStyle windowStyle, bool useShellExecute )
    {
        this.Name = name;
        this.IsExe = isExe;
        this.WindowStyle = windowStyle;
        this.UseShellExecute = useShellExecute;
    }

    // We use shell execute otherwise it displays a console window during one second.
    public static BackstageTool Worker { get; } = new( "Metalama.Backstage.Worker", false, ProcessWindowStyle.Hidden, true );

    public static BackstageTool DesktopWindows { get; } = new( "Metalama.Backstage.Desktop.Windows", true, ProcessWindowStyle.Normal, false );

    public string Name { get; }

    internal bool UseShellExecute { get; }

    internal bool IsExe { get; }

    internal ProcessWindowStyle WindowStyle { get; }

    public override string ToString() => this.Name;
}