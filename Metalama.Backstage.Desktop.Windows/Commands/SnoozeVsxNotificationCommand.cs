// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Spectre.Console.Cli;

namespace Metalama.Backstage.Desktop.Windows.Commands;

internal class SnoozeVsxNotificationCommand : Command<BaseSettings>
{
    public const string Name = "snooze-vsx";

    public override int Execute( CommandContext context, BaseSettings settings ) => 0;
}