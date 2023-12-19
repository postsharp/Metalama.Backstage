// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.UserInterface;

internal class BrowserBasedUserInterfaceService : UserInterfaceService
{
    private readonly IToastNotificationStatusService _toastNotificationStatusService;

    public BrowserBasedUserInterfaceService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._toastNotificationStatusService = serviceProvider.GetRequiredBackstageService<IToastNotificationStatusService>();
    }

    public override void ShowToastNotification( ToastNotification notification, ref bool notificationReported )
    {
        if ( notification.Kind == ToastNotificationKinds.RequiresLicense )
        {
            if ( this._toastNotificationStatusService.TryAcquire( ToastNotificationKinds.RequiresLicense ) )
            {
                _ = this.OpenConfigurationWebPageAsync( "Setup" );
                notificationReported = true;
            }
        }
    }
}