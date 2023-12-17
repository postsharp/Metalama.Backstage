// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Tools;
using System;

namespace Metalama.Backstage.UserInterface;

internal class WindowsToastNotificationService : IToastNotificationService
{
    private readonly IBackstageToolsExecutor _toolsExecutor;
    private readonly IConfigurationManager _configurationManager;
    private readonly IDateTimeProvider _dateTimeProvider;

    public WindowsToastNotificationService( IServiceProvider serviceProvider )
    {
        this._toolsExecutor = serviceProvider.GetRequiredBackstageService<IBackstageToolsExecutor>();
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._dateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
    }

    public void Show( ToastNotification notification )
    {
        // Check if the notification is disabled.
        var kind = notification.Kind.ToString();

        var mustShow = this._configurationManager.UpdateIf<ToastNotificationsConfiguration>(
            c => c.Notifications.TryGetValue( kind, out var kindConfiguration )
                 && !(kindConfiguration.Disabled
                      || (kindConfiguration.SnoozeUntil != null && kindConfiguration.SnoozeUntil > this._dateTimeProvider.Now)),
            c => c with
            {
                Notifications = c.Notifications.SetItem(
                    kind,
                    new ToastNotificationConfiguration() { SnoozeUntil = this._dateTimeProvider.Now + notification.Kind.AutoSnoozePeriod } )
            } );

        if ( !mustShow )
        {
            return;
        }

        // Build arguments.
        var arguments = $"notify {notification.Kind.Name}";

        if ( !string.IsNullOrEmpty( notification.Text ) )
        {
            arguments += $" --text \"{notification.Text}\"";
        }

        // Start the UI process.
        this._toolsExecutor.Start( BackstageTool.DesktopWindows, arguments );
    }

    public void Snooze( ToastNotificationKind kind )
    {
        this._configurationManager.Update<ToastNotificationsConfiguration>(
            config => config with
            {
                Notifications = config.Notifications.SetItem(
                    kind.ToString(),
                    new ToastNotificationConfiguration { SnoozeUntil = this._dateTimeProvider.Now + kind.AutoSnoozePeriod } )
            } );
    }

    public void Disable( ToastNotificationKind kind )
    {
        this._configurationManager.Update<ToastNotificationsConfiguration>(
            config => config with
            {
                Notifications = config.Notifications.SetItem(
                    kind.ToString(),
                    new ToastNotificationConfiguration { Disabled = true } )
            } );
    }
}