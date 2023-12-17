// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Spectre.Console.Cli;
using System.Windows;

namespace Metalama.Backstage.Desktop.Windows.Commands;

internal class InstallVsxCommand : Command<BaseSettings>
{
    public const string Name = "install-vsx";

    public override int Execute( CommandContext context, BaseSettings settings )
    {
        MessageBox.Show( "Installing Vsx" );

        return 0;
    }
}