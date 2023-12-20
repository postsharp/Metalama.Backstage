// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.UserInterface;

internal class ToastNotificationService : IToastNotificationService
{
    private readonly IToastNotificationStatusService _toastNotificationStatusService;
    private readonly IUserInterfaceService _userInterfaceService;

    public ToastNotificationService( IServiceProvider serviceProvider )
    {
        this._toastNotificationStatusService = serviceProvider.GetRequiredBackstageService<IToastNotificationStatusService>();
        this._userInterfaceService = serviceProvider.GetRequiredBackstageService<IUserInterfaceService>();
    }

    public bool Show( ToastNotification notification )
    {
        if ( this._toastNotificationStatusService.TryAcquire( notification.Kind ) )
        {
            var notified = false;
            this._userInterfaceService.ShowToastNotification( notification, ref notified );

            return true;
        }
        else
        {
            return false;
        }
    }
}