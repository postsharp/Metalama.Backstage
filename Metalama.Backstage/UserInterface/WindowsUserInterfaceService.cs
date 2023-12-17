// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.UserInterface;

internal class WindowsUserInterfaceService : IUserInterfaceService
{
    private readonly IToastNotificationService _toastNotificationService;
    private readonly IConfigurationManager _configurationManager;
    private readonly IUserDeviceDetectionService _userDeviceDetectionService;

    private bool _notificationShown;

    public WindowsUserInterfaceService( IServiceProvider serviceProvider )
    {
        this._toastNotificationService = serviceProvider.GetRequiredBackstageService<IToastNotificationService>();
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._userDeviceDetectionService = serviceProvider.GetRequiredBackstageService<IUserDeviceDetectionService>();
    }

    public void OnLicenseMissing()
    {
        if ( this._userDeviceDetectionService.IsInteractiveDevice )
        {
            this._toastNotificationService.Show( new ToastNotification( ToastNotificationKinds.RequiresLicense ) );
            this._notificationShown = true;
        }
    }

    public void Initialize()
    {
        if ( this._userDeviceDetectionService is { IsInteractiveDevice: true, IsVisualStudioInstalled: true } )
        {
            if ( !this._notificationShown &&
                 !this._configurationManager.Get<IdeExtensionsStatusConfiguration>().IsVisualStudioExtensionInstalled )
            {
                this._toastNotificationService.Show( new ToastNotification( ToastNotificationKinds.VsxNotInstalled ) );
            }
        }
    }
}