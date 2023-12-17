// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.UserInterface;
using Microsoft.Toolkit.Uwp.Notifications;
using Spectre.Console.Cli;
using System;
using System.IO;

namespace Metalama.Backstage.Desktop.Windows.Commands;

public sealed class NotificationCommand : Command<NotificationCommandSettings>
{
    public override int Execute( CommandContext context, NotificationCommandSettings settings )
    {
        var builder = new ToastContentBuilder();

        var logoPath = Path.Combine( Path.GetDirectoryName( Environment.GetCommandLineArgs()[0] )!, "logo.png" );
        builder.AddInlineImage( new Uri( "file:///" + logoPath ) );

        var activationArguments = new ActivationArguments( settings );

        if ( settings.Kind == ToastNotificationKinds.RequiresLicense.Name )
        {
            builder.AddArgument( activationArguments.Setup );

            builder.AddVisualChild( new AdaptiveText() { Text = "Activate Metalama", HintStyle = AdaptiveTextStyle.Title } );

            builder.AddVisualChild(
                new AdaptiveText()
                {
                    Text = """
                           Choose between Metalama Free,
                           a 45-day trial of Metalama Ultimate, or register a license key.
                           """,
                    HintMinLines = 4,
                    HintStyle = AdaptiveTextStyle.Body
                } );

            builder.AddButton( "Activate", ToastActivationType.Foreground, activationArguments.Setup );

            builder.SetToastScenario( ToastScenario.Alarm );
        }
        else if ( settings.Kind == ToastNotificationKinds.VsxNotInstalled.Name )
        {
            builder.AddArgument( activationArguments.VsxInstall );

            builder.AddVisualChild( new AdaptiveText() { Text = "Install Metalama Tools for Visual Studio", HintStyle = AdaptiveTextStyle.Header } );

            builder.AddVisualChild(
                new AdaptiveText()
                {
                    Text = """
                           to enhance your Metalama coding experience: syntax highlighting, CodeLens, and diff preview.
                           """,
                    HintStyle = AdaptiveTextStyle.Body,
                    HintMinLines = 4
                } );

            builder.AddButton( "Install", ToastActivationType.Foreground, activationArguments.VsxInstall );
            builder.AddButton( "Later", ToastActivationType.Foreground, activationArguments.VsxSnooze );
            builder.AddButton( "Never", ToastActivationType.Foreground, activationArguments.VsxForget );
        }

        builder.Show();

        return 0;
    }
}