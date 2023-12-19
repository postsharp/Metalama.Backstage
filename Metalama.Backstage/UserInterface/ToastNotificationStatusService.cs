// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using System;

namespace Metalama.Backstage.UserInterface;

/// <summary>
/// The production implementation of <see cref="IToastNotificationStatusService"/>. 
/// </summary>
public class ToastNotificationStatusService : IToastNotificationStatusService
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ToastNotificationStatusService( IServiceProvider serviceProvider )
    {
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._dateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
    }

    public virtual bool CanShow => false;

    private bool IsEnabled( ToastNotificationKind kind, ToastNotificationsConfiguration configuration )
    {
        if ( !configuration.Notifications.TryGetValue( kind.Name, out var kindConfiguration ) )
        {
            // A notification is enabled by default.
            return true;
        }

        if ( kindConfiguration.Disabled )
        {
            return false;
        }

        if ( kindConfiguration.SnoozeUntil != null && kindConfiguration.SnoozeUntil > this._dateTimeProvider.Now )
        {
            return false;
        }

        return true;
    }

    public bool TryAcquire( ToastNotificationKind kind )
        => this._configurationManager.UpdateIf<ToastNotificationsConfiguration>(
            c => this.IsEnabled( kind, c ),
            c => c with
            {
                Notifications = c.Notifications.SetItem(
                    kind.Name,
                    new ToastNotificationConfiguration() { SnoozeUntil = this._dateTimeProvider.Now + kind.AutoSnoozePeriod } )
            } );

    protected virtual void ShowCore( ToastNotification notification ) => throw new NotSupportedException();

    public void Snooze( ToastNotificationKind kind )
        => this._configurationManager.Update<ToastNotificationsConfiguration>(
            config => config with
            {
                Notifications = config.Notifications.SetItem(
                    kind.Name,
                    new ToastNotificationConfiguration { SnoozeUntil = this._dateTimeProvider.Now + kind.ManualSnoozePeriod } )
            } );

    public void Mute( ToastNotificationKind kind )
        => this._configurationManager.Update<ToastNotificationsConfiguration>(
            config => config with
            {
                Notifications = config.Notifications.SetItem(
                    kind.Name,
                    new ToastNotificationConfiguration { Disabled = true } )
            } );
}