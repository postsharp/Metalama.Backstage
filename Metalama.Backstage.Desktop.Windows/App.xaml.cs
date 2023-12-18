// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Desktop.Windows.Commands;
using Metalama.Backstage.Extensibility;
using Microsoft.Toolkit.Uwp.Notifications;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Metalama.Backstage.Desktop.Windows;

public partial class App
{
    public App()
    {
        this.DispatcherUnhandledException += this.OnUnhandledException;
    }

    private void OnUnhandledException( object sender, DispatcherUnhandledExceptionEventArgs e )
    {
        MessageBox.Show( e.Exception.Message );
    }

    public static IServiceProvider GetBackstageServices( BaseSettings settings )
    {
        BackstageServiceFactory.Initialize(
            new BackstageInitializationOptions( new DesktopWindowsApplicationInfo() )
            {
                AddLicensing = true, IsDevelopmentEnvironment = settings.IsDevelopmentEnvironment, AddSupportServices = true, AddUserInterface = true
            },
            "App" );

        return BackstageServiceFactory.ServiceProvider;
    }

    private static Task RunAppAsync( IEnumerable<string> args )
    {
        // Make sure to run the app in a background thread.
        return Task.Run(
            () =>
            {
                CommandApp commandApp = new();

                commandApp.Configure(
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
                                branch.AddCommand<SetupWizardCommand>( SetupWizardCommand.Name );
                            } );
                    } );

                return commandApp.RunAsync( args );
            } );
    }

    protected override void OnStartup( StartupEventArgs e )
    {
        base.OnStartup( e );

        ToastNotificationManagerCompat.OnActivated += this.OnToastNotificationActivated;

        if ( !ToastNotificationManagerCompat.WasCurrentProcessToastActivated() )
        {
            RunAppAsync( e.Args )
                .ContinueWith( _ => this.Dispatcher.BeginInvoke( () => this.Shutdown() ) );
        }
    }

    private void OnToastNotificationActivated( ToastNotificationActivatedEventArgsCompat e )
    {
        RunAppAsync( e.Argument.Split( ' ' ) )
            .ContinueWith(
                _ =>
                {
                    // Remove all notifications from this app.
                    ToastNotificationManagerCompat.History.Clear();

                    // Calling Shutdown does not seem to work.
                    Environment.Exit( 0 );
                } );
    }
}