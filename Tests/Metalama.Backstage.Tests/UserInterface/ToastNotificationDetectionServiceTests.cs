// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.UserInterface;

public class ToastNotificationDetectionServiceTests : TestsBase
{
    private readonly IToastNotificationDetectionService _toastNotificationDetectionService;
    private readonly BackstageServicesInitializer _backstageServicesInitializer; 

    public ToastNotificationDetectionServiceTests( ITestOutputHelper logger ) : base( logger )
    {
        this._toastNotificationDetectionService = this.ServiceProvider.GetRequiredBackstageService<IToastNotificationDetectionService>();
        this._backstageServicesInitializer = this.ServiceProvider.GetRequiredBackstageService<BackstageServicesInitializer>();
    }
    
    [Theory]
    [InlineData( true, true )]
    [InlineData( false, false )]
    public void IsActivationSuggestedOnFirstRun( bool isUserInteractive, bool shouldBeOpened )
    {
        this.UserDeviceDetection.IsInteractiveDevice = isUserInteractive;

        this._backstageServicesInitializer.Initialize();
        this.BackgroundTasks.WhenNoPendingTaskAsync().Wait();
        this._toastNotificationDetectionService.Detect();

        if ( !shouldBeOpened )
        {
            Assert.Empty( this.UserInterface.Notifications );
        }
        else
        {
            Assert.Single( this.UserInterface.Notifications, n => n.Kind == ToastNotificationKinds.RequiresLicense );
        }

        // Initializing a second time should not show a notification because of snoozing.
        this.UserInterface.Notifications.Clear();

        this._toastNotificationDetectionService.Detect();
        Assert.Empty( this.UserInterface.Notifications );

        // After the snooze period, we should see a notification.
        this.Time.AddTime( ToastNotificationKinds.RequiresLicense.AutoSnoozePeriod );

        this._toastNotificationDetectionService.Detect();

        if ( !shouldBeOpened )
        {
            Assert.Empty( this.UserInterface.Notifications );
        }
        else
        {
            Assert.Single( this.UserInterface.Notifications, n => n.Kind == ToastNotificationKinds.RequiresLicense );
        }
    }

    [Fact]
    public void IsUserNotifiedOfTrialExpiration()
    {
        this.UserDeviceDetection.IsInteractiveDevice = true;

        // Register a trial version.
        Assert.True( this.LicenseRegistrationService.TryRegisterTrialEdition( out _ ) );

        // Move the clock to the beginning of the warning period.
        this.Time.AddTime( LicensingConstants.EvaluationPeriod - LicensingConstants.LicenseExpirationWarningPeriod );

        // Initialize
        this._backstageServicesInitializer.Initialize();
        this.BackgroundTasks.WhenNoPendingTaskAsync().Wait();
        this._toastNotificationDetectionService.Detect();

#pragma warning disable CA1307
        Assert.Single( this.UserInterface.Notifications, n => n.Kind == ToastNotificationKinds.TrialExpiring && n.Title?.Contains( "6 days" ) == true );
#pragma warning restore CA1307
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void IsVsxInstallationSuggested( bool extensionInstalled )
    {
        this.UserDeviceDetection.IsInteractiveDevice = true;
        this.UserDeviceDetection.IsVisualStudioInstalled = true;
        this.ServiceProvider.GetRequiredBackstageService<IIdeExtensionStatusService>().IsVisualStudioExtensionInstalled = extensionInstalled;

        // Register a trial version.
        Assert.True( this.LicenseRegistrationService.TryRegisterTrialEdition( out _ ) );

        // Initialize
        this._backstageServicesInitializer.Initialize();
        this.BackgroundTasks.WhenNoPendingTaskAsync().Wait();
        this._toastNotificationDetectionService.Detect();

        if ( extensionInstalled )
        {
            Assert.Empty( this.UserInterface.Notifications );
        }
        else
        {
            Assert.Single( this.UserInterface.Notifications, n => n.Kind == ToastNotificationKinds.VsxNotInstalled );
        }
    }
}