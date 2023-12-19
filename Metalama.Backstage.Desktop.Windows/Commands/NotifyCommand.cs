// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.UserInterface;
using Microsoft.Toolkit.Uwp.Notifications;
using Spectre.Console.Cli;
using System;
using System.IO;

namespace Metalama.Backstage.Desktop.Windows.Commands;

public sealed class NotifyCommand : Command<NotifyCommandSettings>
{
    public const string Name = "notify";

    public override int Execute( CommandContext context, NotifyCommandSettings settings )
    {
        if ( !ViewModelBuilder.TryGetNotificationViewModel( settings, out var notificationViewModel ) )
        {
            return -1;
        }

        var activationArguments = new ActivationArguments( settings );
        var builder = new ToastContentBuilder();

        var logoPath = Path.Combine( Path.GetDirectoryName( Environment.GetCommandLineArgs()[0] )!, "logo.png" );
        builder.AddInlineImage( new Uri( "file:///" + logoPath ) );
        builder.SetToastDuration( ToastDuration.Long );

        builder.AddVisualChild( new AdaptiveText() { Text = notificationViewModel.Title, HintStyle = AdaptiveTextStyle.Title } );

        builder.AddVisualChild( new AdaptiveText() { Text = notificationViewModel.Body, HintMinLines = 4, HintStyle = AdaptiveTextStyle.Body } );

        switch ( notificationViewModel.Action )
        {
            case CommandActionViewModel commandAction:
                builder.AddArgument( commandAction.Command );
                builder.AddButton( commandAction.Text, ToastActivationType.Foreground, commandAction.Command );

                break;

            case OpenWebPageActionViewModel openWebPageAction:
                builder.SetProtocolActivation( new Uri( openWebPageAction.Url ) );
                builder.AddButton( openWebPageAction.Text, ToastActivationType.Protocol, openWebPageAction.Url );

                break;
        }

        builder.AddButton( "Snooze", ToastActivationType.Foreground, activationArguments.Snooze );
        builder.AddButton( "Mute", ToastActivationType.Foreground, activationArguments.Mute );

        builder.Show();

        return 0;
    }
}