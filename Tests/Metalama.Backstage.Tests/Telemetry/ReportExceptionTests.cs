// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Telemetry;

public class ReportExceptionTests : TestsBase
{
    public ReportExceptionTests( ITestOutputHelper logger ) : base(
        logger,
        builder => builder.AddConfigurationManager().AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() ) ) ) { }

    [Fact]
    public async Task ShouldReportExceptionConcurrent()
    {
        for ( var i = 0; i < 50; i++ )
        {
            var hash = Guid.NewGuid().ToString();

            bool ShouldReportIssue()
            {
                var reporter = new ExceptionReporter( new TelemetryQueue( this.ServiceProvider ), this.ServiceProvider );

                return reporter.ShouldReportIssue( hash );
            }

            this.Logger.WriteLine( $"------------------- {i + 1} ---------------- " );
            var tasks = new[] { Task.Run( ShouldReportIssue ), Task.Run( ShouldReportIssue ) };
            await Task.WhenAll( tasks );

            var trueCount = tasks.Count( t => t.Result );

            Assert.Equal( 1, trueCount );
        }
    }
}