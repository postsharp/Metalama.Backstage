// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.UserInterface;

internal class BrowserBasedUserInterfaceService : UserInterfaceService
{
    private readonly IToastNotificationService _toastNotificationService;

    public BrowserBasedUserInterfaceService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        this._toastNotificationService = serviceProvider.GetRequiredBackstageService<IToastNotificationService>();
    }

    protected override void Notify( ToastNotificationKind kind, ref bool notificationReported )
    {
        if ( kind == ToastNotificationKinds.RequiresLicense )
        {
            if ( this._toastNotificationService.TryAcquire( ToastNotificationKinds.RequiresLicense ) )
            {
                _ = this.OpenConfigurationWebPageAsync( "Setup" );
                notificationReported = true;
            }
        }
    }
}