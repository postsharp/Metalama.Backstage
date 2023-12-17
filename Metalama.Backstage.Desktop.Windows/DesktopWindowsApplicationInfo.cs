﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;

namespace Metalama.Backstage.Desktop.Windows;

internal class DesktopWindowsApplicationInfo : ApplicationInfoBase
{
    public DesktopWindowsApplicationInfo() : base( typeof(DesktopWindowsApplicationInfo).Assembly ) { }

    public override string Name => "Metalama.Backstage.Desktop.Windows";
}