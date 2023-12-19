// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Spectre.Console.Cli;

namespace Metalama.Backstage.Desktop.Windows.Commands;

internal class MuteNotificationCommandSettings : BaseSettings
{
    [CommandArgument( 0, "<kind>" )]
    public string Kind { get; init; }
}