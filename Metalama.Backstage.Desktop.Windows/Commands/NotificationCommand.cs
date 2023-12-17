// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Notifications;
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

        switch ( settings.Kind )
        {
            case ToastNotificationKind.RequiresLicense:
                builder.AddArgument( ActivationArguments.LicenseActivate );

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

                builder.AddButton( "Activate", ToastActivationType.Foreground, ActivationArguments.LicenseActivate );

                builder.SetToastScenario( ToastScenario.Alarm );

                break;

            case ToastNotificationKind.VsxNotInstalled:
                builder.AddArgument( ActivationArguments.VsxInstall );

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

                builder.AddButton( "Install", ToastActivationType.Foreground, ActivationArguments.VsxInstall );
                builder.AddButton( "Later", ToastActivationType.Foreground, ActivationArguments.VsxSnooze );
                builder.AddButton( "Never", ToastActivationType.Foreground, ActivationArguments.VsxForget );

                break;
        }

        builder.Show();

        return 0;
    }
}