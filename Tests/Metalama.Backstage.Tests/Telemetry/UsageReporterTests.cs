// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Telemetry.Metrics;
using Metalama.Backstage.Testing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Telemetry;

public class UsageReporterTests : TestsBase
{
    private readonly UsageReporter _reporter;

    public UsageReporterTests( ITestOutputHelper logger ) : base(
        logger,
        services =>
            services
                .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() { IsTelemetryEnabled = true } ) )
                .AddSingleton<ITelemetryConfigurationService>( new TelemetryConfigurationService( services.ServiceProvider ) )
                .AddSingleton<ITelemetryUploader>( new NullTelemetryUploader() ) )
    {
        this._reporter = new UsageReporter( this.ServiceProvider );
    }

    private void ReportSession( string kind = "TestSession" )
    {
        var session = this._reporter.StartSession( kind ); 
        Assert.NotNull( session );
        Assert.NotEmpty( session!.Metrics );
        
        session.Dispose();

        Assert.Throws<InvalidOperationException>( () => session.Metrics );
        Assert.Single( this.FileSystem.Mock.AllFiles );
    }

    private void AssertReportingDisabled()
    {
        // We can't use the reporter from the constructor, because it's been created with the wrong configuration.
        var reporter = new UsageReporter( this.ServiceProvider );
        
        Assert.False( reporter.ShouldReportSession( "TestProject" ) );
        
        Assert.Null( reporter.StartSession( "TestSession" ) );
        Assert.Empty( this.FileSystem.Mock.AllFiles );
    }

    [Fact]
    public void UsageIsReportedWhenTelemetryIsEnabled()
    {
        this.ReportSession();
    }
    
    [Fact]
    public void UsageIsNotReportedWhenTelemetryIsDisabled()
    {
        ((TestApplicationInfo) this.ServiceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication).IsTelemetryEnabled = false;
        this.AssertReportingDisabled();
    }
    
    [Fact]
    public void UsageIsNotReportedWhenOptOutEnvironmentVariableIsSet()
    {
        this.EnvironmentVariableProvider.Environment["METALAMA_TELEMETRY_OPT_OUT"] = "true";
        this.AssertReportingDisabled();
    }
    
    [Fact]
    public void UsageRepostingCanBeRepeatedWithoutShouldReportSessionCheck()
    {
        this.ReportSession();
        this.FileSystem.DeleteFile( this.FileSystem.Mock.AllFiles.Single() );
        this.ReportSession();
    }

    private void AssertSessionShouldBeReported( string projectName = "TestProject" ) => Assert.True( this._reporter.ShouldReportSession( projectName ) );
    
    private void AssertSessionShouldNotBeReported( string projectName = "TestProject" ) => Assert.False( this._reporter.ShouldReportSession( projectName ) );
    
    [Fact]
    public void FirstSessionSoShouldBeReported()
    {
        this.AssertSessionShouldBeReported();
    }

    [Fact]
    public void SessionShouldNotBeReportedWhenReportedRecently()
    {
        this.AssertSessionShouldBeReported();
        this.AssertSessionShouldNotBeReported();
        this.Time.AddTime( TimeSpan.FromDays( 1 ).Add( -TimeSpan.FromMinutes( 1 ) ) );
        this.AssertSessionShouldNotBeReported();
    }
    
    [Fact]
    public void SessionShouldBeReportedAfterOneDay()
    {
        this.AssertSessionShouldBeReported();
        this.Time.AddTime( TimeSpan.FromDays( 1 ) );
        this.AssertSessionShouldBeReported();
        this.Time.AddTime( TimeSpan.FromDays( 1 ) );
        this.AssertSessionShouldBeReported();
    }

    [Fact]
    public void SessionShouldBeReportedAfterOneDayEvenWhenOtherProjectsReported()
    {
        this.AssertSessionShouldBeReported( "TestProject1" );
        this.AssertSessionShouldNotBeReported( "TestProject1" );
        this.AssertSessionShouldBeReported( "TestProject2" );
        this.AssertSessionShouldNotBeReported( "TestProject2" );
        this.Time.AddTime( TimeSpan.FromDays( 1 ) );
        this.AssertSessionShouldBeReported( "TestProject1" );
        this.AssertSessionShouldNotBeReported( "TestProject1" );
        this.AssertSessionShouldBeReported( "TestProject2" );
        this.AssertSessionShouldNotBeReported( "TestProject2" );
    }

    [Fact]
    public void SessionShouldBeCleanedUpAfterOneDay()
    {
        void AssertSessionsCount( int count ) => Assert.Equal( count, this.ConfigurationManager.Get<TelemetryConfiguration>().Sessions.Count );

        AssertSessionsCount( 0 );
        this.AssertSessionShouldBeReported( "TestProject1" );
        AssertSessionsCount( 1 );
        this.AssertSessionShouldBeReported( "TestProject2" );
        AssertSessionsCount( 2 );
        this.Time.AddTime( TimeSpan.FromDays( 1 ) );
        this.AssertSessionShouldBeReported( "TestProject1" );
        AssertSessionsCount( 1 );
    }

    [Fact]
    public async Task SessionsCanBeReportedConcurrentlyAsync()
    {
        var event1 = new SemaphoreSlim( 0 );
        var event2 = new SemaphoreSlim( 0 );
        
        async Task<IDisposable> StartSession( string projectName, SemaphoreSlim e )
        {
            var session = this._reporter.StartSession( "TestSession" );
            Assert.NotNull( session );
            session!.Metrics.Add( new StringMetric( "ProjectName", projectName ) );

            await e.WaitAsync();
            
            Assert.Single( session.Metrics, m => m is StringMetric stringMetric && stringMetric.Value == projectName );
            
            return session;
        }

        var session1Task = StartSession( "TestProject1", event1 );
        var session2Task = StartSession( "TestProject2", event2 );

        event1.Release();
        await session1Task;
        
        event2.Release();
        await session2Task;
        
        session1Task.Result.Dispose();
        session2Task.Result.Dispose();

        Assert.Equal( 2, this.FileSystem.Mock.AllFiles.Count() );
    }
}