// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using Metalama.Backstage.UserInterface;
using Metalama.Backstage.Welcome;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.UserInterface;

public class WelcomeServiceTests : TestsBase
{
    public WelcomeServiceTests( ITestOutputHelper logger ) : base( logger ) { }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void TelemetryIsConfigured( bool isTelemetryDisabled )
    {
        void SetTelemetry( ReportingAction action )
        {
            this.ConfigurationManager!.Update<TelemetryConfiguration>(
                c => c with { ExceptionReportingAction = action, UsageReportingAction = action, PerformanceProblemReportingAction = action } );
        }

        void CheckTelemetry( ReportingAction expectedAction )
        {
            var telemetryConfiguration = this.ConfigurationManager!.Get<TelemetryConfiguration>();
            var testedPropertiesCount = 0;

            foreach ( var property in telemetryConfiguration.GetType().GetProperties() )
            {
                if ( property.PropertyType == typeof(ReportingAction) )
                {
                    Assert.Equal( expectedAction, property.GetValue( telemetryConfiguration ) );
                    testedPropertiesCount++;
                }
            }

            Assert.NotEqual( 0, testedPropertiesCount );
        }

        if ( isTelemetryDisabled )
        {
            SetTelemetry( ReportingAction.No );
        }

        var welcomeService = this.ServiceProvider.GetRequiredBackstageService<WelcomeService>();
        welcomeService.Initialize();
        CheckTelemetry( isTelemetryDisabled ? ReportingAction.No : ReportingAction.Yes );

        // Reset telemetry before second run
        SetTelemetry( ReportingAction.Ask );

        // Second run
        welcomeService.Initialize();
        CheckTelemetry( ReportingAction.Ask );
    }

    [Theory]
    [InlineData( true, true )]
    [InlineData( false, false )]
    public void IsSetupPageOpenedOnFirstRun( bool isUserInteractive, bool shouldBeOpened )
    {
        this.UserDeviceDetection.IsInteractiveDevice = isUserInteractive;

        var initializerService = this.ServiceProvider.GetRequiredBackstageService<BackstageServicesInitializer>();

        initializerService.Initialize();

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
    public void TrialExpiring()
    {
        this.UserDeviceDetection.IsInteractiveDevice = true;

        // Register a trial version.
        Assert.True( this.ServiceProvider.GetRequiredBackstageService<ILicenseRegistrationService>().TryRegisterTrialEdition( out _ ) );

        // Move the clock to the beginning of the warning period.
        this.Time.AddTime( LicensingConstants.EvaluationPeriod - LicensingConstants.LicenseExpirationWarningPeriod );

        // Initialize
        var initializerService = this.ServiceProvider.GetRequiredBackstageService<BackstageServicesInitializer>();
        initializerService.Initialize();

#pragma warning disable CA1307
        Assert.Single( this.UserInterface.Notifications, n => n.Kind == ToastNotificationKinds.TrialExpiring && n.Title?.Contains( "6 days" ) == true );
#pragma warning restore CA1307
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void VsxNotInstalled( bool extensionInstalled )
    {
        this.UserDeviceDetection.IsInteractiveDevice = true;
        this.UserDeviceDetection.IsVisualStudioInstalled = true;
        this.ServiceProvider.GetRequiredBackstageService<IIdeExtensionStatusService>().IsVisualStudioExtensionInstalled = extensionInstalled;

        // Register a trial version.
        Assert.True( this.ServiceProvider.GetRequiredBackstageService<ILicenseRegistrationService>().TryRegisterTrialEdition( out _ ) );

        // Initialize
        var initializerService = this.ServiceProvider.GetRequiredBackstageService<BackstageServicesInitializer>();
        initializerService.Initialize();

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