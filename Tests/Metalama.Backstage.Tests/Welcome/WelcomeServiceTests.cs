// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Welcome;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Welcome;

public class WelcomeServiceTests : TestsBase
{
    private const int _maxRecentUserInteractionMinutes = 14;
    private const int _minLargeScreenWidth = 1280;

    private readonly TestApplicationInfo _applicationInfo;

    public WelcomeServiceTests( ITestOutputHelper logger ) : base(
        logger,
        builder => builder
            .AddSingleton<IEnvironmentVariableProvider>( new TestEnvironmentVariableProvider() )
            .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() ) ) )
    {
        this._applicationInfo = (TestApplicationInfo) this.ServiceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
    }

    [Theory]
    [InlineData( false, false, false, true )]
    [InlineData( true, false, false, false )]
    [InlineData( false, true, false, false )]
    [InlineData( false, false, true, false )]
    public void EvaluationLicenseIsRegisteredOnFirstRun(
        bool ignoreUserProfileLicenses,
        bool isPrerelease,
        bool isUnattendedProcess,
        bool expectLicenseRegistered )
    {
        ILicense? GetLicense()
        {
            var licenseSource = new UserProfileLicenseSource( this.ServiceProvider );
            var messages = new List<LicensingMessage>();
            var license = licenseSource.GetLicense( m => messages.Add( m ) );
            Assert.Empty( messages );

            return license;
        }

        this._applicationInfo.IsPrerelease = isPrerelease;
        this._applicationInfo.IsUnattendedProcess = isUnattendedProcess;

        var options = new BackstageInitializationOptions( this._applicationInfo, "TestProject" )
        {
            LicensingOptions = new LicensingInitializationOptions { IgnoreUserProfileLicenses = ignoreUserProfileLicenses }
        };

        var welcomeService = new WelcomeService( this.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup( options );

        var license = GetLicense();

        if ( expectLicenseRegistered )
        {
            Assert.NotNull( license );
            Assert.True( license!.TryGetLicenseConsumptionData( out var data, out var errorMessage ) );
            Assert.Null( errorMessage );
            Assert.Equal( LicenseType.Evaluation, data!.LicenseType );

            // Unregister the license before second run.
            this.ConfigurationManager.Update<LicensingConfiguration>( c => c with { License = null } );
        }
        else
        {
            Assert.Null( license );
        }

        // Second run
        welcomeService = new WelcomeService( this.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup( options );
        license = GetLicense();
        Assert.Null( license );
    }

    [Theory]
    [InlineData( true, true )]
    [InlineData( true, false )]
    [InlineData( false, true )]
    [InlineData( false, false )]
    public void TelemetryIsConfigured( bool registerEvaluationLicense, bool isTelemetryDisabled )
    {
        void SetTelemetry( ReportingAction action )
        {
            this.ConfigurationManager.Update<TelemetryConfiguration>(
                c => c with { ExceptionReportingAction = action, UsageReportingAction = action, PerformanceProblemReportingAction = action } );
        }

        void CheckTelemetry( ReportingAction expectedAction )
        {
            var telemetryConfiguration = this.ConfigurationManager.Get<TelemetryConfiguration>();
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

        var welcomeService = new WelcomeService( this.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup( registerEvaluationLicense );
        CheckTelemetry( isTelemetryDisabled ? ReportingAction.No : ReportingAction.Yes );

        // Reset telemetry before second run
        SetTelemetry( ReportingAction.Ask );

        // Second run
        welcomeService = new WelcomeService( this.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup( registerEvaluationLicense );
        CheckTelemetry( ReportingAction.Ask );
    }

    [Theory]
    [InlineData( true, true, _maxRecentUserInteractionMinutes, _minLargeScreenWidth, true )]
    [InlineData( true, false, _maxRecentUserInteractionMinutes, _minLargeScreenWidth, true )]
    [InlineData( false, true, _maxRecentUserInteractionMinutes, _minLargeScreenWidth, true )]
    [InlineData( false, false, _maxRecentUserInteractionMinutes, _minLargeScreenWidth, true )]
    [InlineData( true, false, _maxRecentUserInteractionMinutes + 1, _minLargeScreenWidth, false )]
    [InlineData( true, false, _maxRecentUserInteractionMinutes, _minLargeScreenWidth - 1, false )]
    [InlineData( true, false, _maxRecentUserInteractionMinutes + 1, _minLargeScreenWidth - 1, false )]
    public void IsWelcomePageOpenedOnFirstRun(
        bool registerEvaluationLicense,
        bool isPrerelease,
        int? lastInputTimeMinutes,
        int? totalMonitorWidth,
        bool shouldBeOpened )
    {
        this._applicationInfo.IsPrerelease = isPrerelease;
        this.UserInteraction.LastInputTime = lastInputTimeMinutes == null ? null : TimeSpan.FromMinutes( lastInputTimeMinutes.Value );
        this.UserInteraction.TotalMonitorWidth = totalMonitorWidth;

        var welcomeService = new WelcomeService( this.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup( registerEvaluationLicense );

        void AssertNotOpened() => Assert.Empty( this.ProcessExecutor.StartedProcesses );
        
        if ( !shouldBeOpened )
        {
            AssertNotOpened();

            return;
        }

        Assert.Single( this.ProcessExecutor.StartedProcesses );
        var processName = this.ProcessExecutor.StartedProcesses.Single().FileName;

        if ( isPrerelease )
        {
            Assert.Contains( "preview", processName, StringComparison.Ordinal );
        }
        else
        {
            Assert.DoesNotContain( "preview", processName, StringComparison.Ordinal );
        }

        // Reset the list of processes before second run
        this.ProcessExecutor.StartedProcesses.Clear();

        // Second run
        welcomeService = new WelcomeService( this.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup( registerEvaluationLicense );

        AssertNotOpened();
    }
}