// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Welcome;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Welcome;

public class WelcomeServiceTests : TestsBase
{
    private readonly TestApplicationInfo _applicationInfo;

    public WelcomeServiceTests( ITestOutputHelper logger ) : base( logger )
    {
        this._applicationInfo = (TestApplicationInfo) this.ServiceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
    }

    protected override void ConfigureServices( ServiceProviderBuilder services )
    {
        services
            .AddSingleton<IEnvironmentVariableProvider>( new TestEnvironmentVariableProvider() )
            .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() ) );
    }

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

        var welcomeService = new WelcomeService( this.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup();
        CheckTelemetry( isTelemetryDisabled ? ReportingAction.No : ReportingAction.Yes );

        // Reset telemetry before second run
        SetTelemetry( ReportingAction.Ask );

        // Second run
        welcomeService = new WelcomeService( this.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup();
        CheckTelemetry( ReportingAction.Ask );
    }

    [Theory]
    [InlineData( true, true, true )]
    [InlineData( false, true, true )]
    [InlineData( false, false, false )]
    public void IsWelcomePageOpenedOnFirstRun(
        bool isPrerelease,
        bool isUserInteractive,
        bool shouldBeOpened )
    {
        this._applicationInfo.IsPrerelease = isPrerelease;
        this.UserDeviceDetection.IsInteractiveDevice = isUserInteractive;

        var welcomeService = new WelcomeService( this.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup();

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
        welcomeService.ExecuteFirstStartSetup();

        AssertNotOpened();
    }
}