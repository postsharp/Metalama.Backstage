// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.UserInterface;
using Spectre.Console.Cli;
using System;

namespace Metalama.Backstage.Desktop.Windows.Commands;

internal class SnoozeNotificationCommand : Command<MuteNotificationCommandSettings>
{
    public const string Name = "snooze";

    public override int Execute( CommandContext context, MuteNotificationCommandSettings settings )
    {
        var serviceProvider = App.GetBackstageServices( settings );

        if ( !ToastNotificationKinds.All.TryGetValue( settings.Kind, out var kind ) )
        {
            Console.WriteLine( $"Invalid notification kind: {settings.Kind}." );

            return 1;
        }

        serviceProvider.GetRequiredBackstageService<IToastNotificationConfigurationService>().Snooze( kind );

        return 0;
    }
}