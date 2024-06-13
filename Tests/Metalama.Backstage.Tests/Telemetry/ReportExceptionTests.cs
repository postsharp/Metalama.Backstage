// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Telemetry;

public class ReportExceptionTests : TestsBase
{
    public ReportExceptionTests( ITestOutputHelper logger ) : base( logger )
    {
        this.ConfigurationManager!.Update<TelemetryConfiguration>(
            c => c with { ExceptionReportingAction = ReportingAction.Yes, PerformanceProblemReportingAction = ReportingAction.Yes } );
    }

    protected override void ConfigureServices( ServiceProviderBuilder services )
    {
        services.AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() { IsTelemetryEnabled = true } ) );
    }

    [Fact]
    public async Task ShouldReportExceptionConcurrent()
    {
        for ( var i = 0; i < 50; i++ )
        {
            var hash = Guid.NewGuid().ToString();

            bool ShouldReportIssue()
            {
                // To simulate a multi-process situation, each iteration of the test should have its own ConfigurationManager.
                var serviceProvider = this.CloneServiceCollection()
                    .AddSingleton<IConfigurationManager>( new Configuration.ConfigurationManager( this.ServiceProvider ) )
                    .BuildServiceProvider();

                var reporter = new ExceptionReporter( new TelemetryQueue( serviceProvider ), serviceProvider );

                return reporter.ShouldReportIssue( hash );
            }

            this.Logger.WriteLine( $"------------------- {i + 1} ---------------- " );
            var tasks = Enumerable.Range( 0, 10 ).Select( _ => Task.Run( ShouldReportIssue ) ).ToList();
            await Task.WhenAll( tasks );

            var trueCount = tasks.Count( t => t.Result );

            this.Logger.WriteLine( $"Tasks that managed to report the issue: {trueCount}" );

            Assert.Equal( 1, trueCount );
        }
    }

    [Fact]
    public void ShouldReportException()
    {
        var reporter = new ExceptionReporter( new TelemetryQueue( this.ServiceProvider ), this.ServiceProvider );
        Assert.False( reporter.ShouldReportException( new TaskCanceledException() ) );
        Assert.False( reporter.ShouldReportException( new OperationCanceledException() ) );
        Assert.False( reporter.ShouldReportException( new IOException() ) );
        Assert.False( reporter.ShouldReportException( new UnauthorizedAccessException() ) );
        Assert.False( reporter.ShouldReportException( new WebException() ) );
        Assert.False( reporter.ShouldReportException( new AggregateException( new IOException() ) ) );
        Assert.False( reporter.ShouldReportException( new InvalidOperationException( "", new IOException() ) ) );
        Assert.True( reporter.ShouldReportException( new InvalidOperationException( "" ) ) );
    }

    private void ReportException(
        ReportingAction exceptionReportingAction = ReportingAction.Yes,
        ReportingAction performanceReportingAction = ReportingAction.Yes,
        ExceptionReportingKind exceptionReportingKind = ExceptionReportingKind.Exception )
    {
        this.ConfigurationManager!.Update<TelemetryConfiguration>(
            c => c with { ExceptionReportingAction = exceptionReportingAction, PerformanceProblemReportingAction = performanceReportingAction } );

        var reporter = new ExceptionReporter( new TelemetryQueue( this.ServiceProvider ), this.ServiceProvider );
        reporter.ReportException( new InvalidOperationException(), exceptionReportingKind );
    }

    [Theory]
    [InlineData( ReportingAction.Yes, ReportingAction.No, ExceptionReportingKind.Exception, true )]
    [InlineData( ReportingAction.No, ReportingAction.Yes, ExceptionReportingKind.Exception, false )]
    [InlineData( ReportingAction.Ask, ReportingAction.Yes, ExceptionReportingKind.Exception, false )]
    [InlineData( ReportingAction.No, ReportingAction.Yes, ExceptionReportingKind.PerformanceProblem, true )]
    [InlineData( ReportingAction.Yes, ReportingAction.No, ExceptionReportingKind.PerformanceProblem, false )]
    [InlineData( ReportingAction.Yes, ReportingAction.Ask, ExceptionReportingKind.PerformanceProblem, false )]
    public void ExceptionsAreReportedAsConfiguredWhenTelemetryIsEnabled(
        ReportingAction exceptionReportingAction,
        ReportingAction performanceReportingAction,
        ExceptionReportingKind exceptionReportingKind,
        bool shouldReport )
    {
        this.ReportException( exceptionReportingAction, performanceReportingAction, exceptionReportingKind );

        if ( shouldReport )
        {
            Assert.Single( this.FileSystem.Mock.AllFiles );

            // Check that the result is valid XML.
            var xml = this.FileSystem.ReadAllText( this.FileSystem.Mock.AllFiles.Single() );
            _ = XDocument.Parse( xml );
        }
        else
        {
            Assert.Empty( this.FileSystem.Mock.AllFiles );
        }
    }

    private void AssertReportingDisabled()
    {
        this.ReportException();
        Assert.Empty( this.FileSystem.Mock.AllFiles );
    }

    [Fact]
    public void ExceptionsAreNotReportedWhenTelemetryIsDisabled()
    {
        ((TestApplicationInfo) this.ServiceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication).IsTelemetryEnabled = false;
        this.AssertReportingDisabled();
    }
    
    [Fact]
    public void ExceptionsAreNotReportedWhenOptOutEnvironmentVariableIsSet()
    {
        this.EnvironmentVariableProvider.Environment["METALAMA_TELEMETRY_OPT_OUT"] = "true";
        this.AssertReportingDisabled();
    }

    [Fact]
    public void ExceptionsAreNotReportedForUnattendedBuild()
    {
        ((TestApplicationInfo) this.ServiceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication).IsUnattendedProcess = true;
        this.AssertReportingDisabled();
    }
}