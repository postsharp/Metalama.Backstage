﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using System;

namespace Metalama.Backstage.UserInterface;

internal class ToastNotificationDetectionService : IBackstageService
{
    private readonly IToastNotificationService _toastNotificationService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUserDeviceDetectionService _userDeviceDetectionService;
    private readonly IIdeExtensionStatusService? _ideExtensionStatusService;
    private readonly ILicenseRegistrationService? _licenseRegistrationService;
    private readonly object _initializationSync = new();

    private DateTime _lastTimeInitialized;

    public ToastNotificationDetectionService( IServiceProvider serviceProvider )
    {
        this._licenseRegistrationService = serviceProvider.GetBackstageService<ILicenseRegistrationService>();
        this._userDeviceDetectionService = serviceProvider.GetRequiredBackstageService<IUserDeviceDetectionService>();
        this._dateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._ideExtensionStatusService = serviceProvider.GetBackstageService<IIdeExtensionStatusService>();
        this._toastNotificationService = serviceProvider.GetRequiredBackstageService<IToastNotificationService>();
    }

    private string FormatExpiration( DateTime expiration )
    {
        var daysToExpiration = (int) Math.Floor( (expiration - this._dateTimeProvider.Now).TotalDays );

        return daysToExpiration switch
        {
            0 => "today",
            1 => "tomorrow",
            _ => $"in {daysToExpiration} days"
        };
    }

    private void ValidateRegisteredLicense( LicenseProperties? license, ref bool notificationReported )
    {
        // We set notificationReported to true even if the notification is not reported because of snoozing
        // because the reason of this flag is to avoid displaying VsxNotInstalled.
        
        if ( license == null )
        {
            this._toastNotificationService.Show( new ToastNotification( ToastNotificationKinds.RequiresLicense ) );
            
            notificationReported = true;
        }
        else
        {
            if ( license is { ValidTo: not null }
                 && license.ValidTo.Value.Subtract( LicensingConstants.LicenseExpirationWarningPeriod ) < this._dateTimeProvider.Now )
            {
                if ( license.LicenseType == LicenseType.Evaluation )
                {
                    this._toastNotificationService.Show(
                        new ToastNotification(
                            ToastNotificationKinds.TrialExpiring,
                            $"Your Metalama trial expires {this.FormatExpiration( license.ValidTo.Value )}",
                            "Switch to Metalama [Free] or register a license key to avoid loosing functionality." ) );
                }
                else
                {
                    this._toastNotificationService.Show(
                        new ToastNotification(
                            ToastNotificationKinds.LicenseExpiring,
                            $"Your Metalama license expires {this.FormatExpiration( license.ValidTo.Value )}",
                            "Register a new license license key  to avoid loosing functionality." ) );
                }
                
                notificationReported = true;
            }
            else if ( license is { SubscriptionEndDate: not null }
                      && license.SubscriptionEndDate.Value.Subtract( LicensingConstants.SubscriptionExpirationWarningPeriod ) < this._dateTimeProvider.Now )
            {
                this._toastNotificationService.Show(
                    new ToastNotification(
                        ToastNotificationKinds.SubscriptionExpiring,
                        $"Your Metalama subscription expires {this.FormatExpiration( license.SubscriptionEndDate.Value )}",
                        "Renew your subscription and register a new license key to continue benefiting from updates." ) );
                
                notificationReported = true;
            }
        }
    }

    public void Detect()
    {
        // Avoid too frequent initializations for performance reasons. The threshold (here 15 seconds) should be lower
        // than the lowest auto-snooze period of a toast notification.
        lock ( this._initializationSync )
        {
            if ( this._lastTimeInitialized > this._dateTimeProvider.Now.Subtract( TimeSpan.FromSeconds( 15 ) ) )
            {
                return;
            }

            this._lastTimeInitialized = this._dateTimeProvider.Now;
        }

        var notificationReported = false;

        // Validate the current license.
        if ( this._userDeviceDetectionService.IsInteractiveDevice )
        {
            if ( this._licenseRegistrationService != null )
            {
                this.ValidateRegisteredLicense( this._licenseRegistrationService.RegisteredLicense, ref notificationReported );
            }

            if ( !notificationReported && this._ideExtensionStatusService?.ShouldRecommendToInstallVisualStudioExtension == true )
            {
                this._toastNotificationService.Show( new ToastNotification( ToastNotificationKinds.VsxNotInstalled ) );
            }
        }
    }
}