// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using System.Threading.Tasks;
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

    private async Task DetectToastNotificationsAsync()
    {
        this._toastNotificationDetectionService.Detect();
        await this.BackgroundTasks.WhenNoPendingTaskAsync();
    }
    
    [Theory]
    [InlineData( true, true )]
    [InlineData( false, false )]
    public async Task IsActivationSuggestedOnFirstRunAsync( bool isUserInteractive, bool shouldBeOpened )
    {
        this.UserDeviceDetection.IsInteractiveDevice = isUserInteractive;

        this._backstageServicesInitializer.Initialize();
        await this.DetectToastNotificationsAsync();

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

        await this.DetectToastNotificationsAsync();
        Assert.Empty( this.UserInterface.Notifications );

        // After the snooze period, we should see a notification.
        this.Time.AddTime( ToastNotificationKinds.RequiresLicense.AutoSnoozePeriod );

        await this.DetectToastNotificationsAsync();

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
    public async Task IsUserNotifiedOfTrialExpirationAsync()
    {
        this.UserDeviceDetection.IsInteractiveDevice = true;

        // Register a trial version.
        Assert.True( this.LicenseRegistrationService.TryRegisterTrialEdition( out _ ) );

        // Move the clock to the beginning of the warning period.
        this.Time.AddTime( LicensingConstants.EvaluationPeriod - LicensingConstants.LicenseExpirationWarningPeriod );

        // Initialize
        this._backstageServicesInitializer.Initialize();
        await this.DetectToastNotificationsAsync();

#pragma warning disable CA1307
        Assert.Single( this.UserInterface.Notifications, n => n.Kind == ToastNotificationKinds.TrialExpiring && n.Title?.Contains( "6 days" ) == true );
#pragma warning restore CA1307
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public async Task IsVsxInstallationSuggestedAsync( bool extensionInstalled )
    {
        this.UserDeviceDetection.IsInteractiveDevice = true;
        this.UserDeviceDetection.IsVisualStudioInstalled = true;
        this.ServiceProvider.GetRequiredBackstageService<IIdeExtensionStatusService>().IsVisualStudioExtensionInstalled = extensionInstalled;

        // Register a trial version.
        Assert.True( this.LicenseRegistrationService.TryRegisterTrialEdition( out _ ) );

        // Initialize
        this._backstageServicesInitializer.Initialize();
        await this.DetectToastNotificationsAsync();

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