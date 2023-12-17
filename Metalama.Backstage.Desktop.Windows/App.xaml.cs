// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Desktop.Windows.Commands;
using Microsoft.Toolkit.Uwp.Notifications;
using Spectre.Console.Cli;
using System;
using System.Windows;

namespace Metalama.Backstage.Desktop.Windows;

public partial class App
{
    private readonly CommandApp _commandApp = new();

    public App()
    {
        this._commandApp.Configure(
            configuration =>
            {
                configuration.AddCommand<NotificationCommand>( "notify" );

                configuration.AddBranch(
                    "activate",
                    branch =>
                    {
                        branch.AddCommand<InstallVsxCommand>( InstallVsxCommand.Name );
                        branch.AddCommand<SnoozeVsxNotificationCommand>( SnoozeVsxNotificationCommand.Name );
                        branch.AddCommand<DismissVsxNotificationCommand>( DismissVsxNotificationCommand.Name );
                    } );
            } );
    }

    protected override void OnStartup( StartupEventArgs e )
    {
        base.OnStartup( e );

        ToastNotificationManagerCompat.OnActivated += this.OnToastNotificationActivated;

        if ( !ToastNotificationManagerCompat.WasCurrentProcessToastActivated() )
        {
            this._commandApp.Run( e.Args );

            this.Shutdown();
        }
    }

    private void OnToastNotificationActivated( ToastNotificationActivatedEventArgsCompat e )
    {
        this._commandApp.Run( e.Argument.Split( ' ' ) );

        // Remove all notifications from this app.
        ToastNotificationManagerCompat.History.Clear();

        // Calling Shutdown does not seem to work.
        Environment.Exit( 0 );
    }
}