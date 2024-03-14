// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
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
}