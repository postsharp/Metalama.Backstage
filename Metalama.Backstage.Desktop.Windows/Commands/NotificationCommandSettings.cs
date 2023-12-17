// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Notifications;
using Spectre.Console.Cli;

namespace Metalama.Backstage.Desktop.Windows.Commands;

public class NotificationCommandSettings : BaseSettings
{
    [CommandArgument( 0, "<kind>" )]
    public ToastNotificationKind Kind { get; set; }

    [CommandOption( "--text" )]
    public string? Text { get; set; }
}