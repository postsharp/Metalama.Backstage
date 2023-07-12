// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NET

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Welcome;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Telemetry;

public class TelemetryUploaderTests : TestsBase
{
    private const string _feedbackDirectory = @"C:\feedback";

    private readonly TelemetryTestsPutMessageHandler _httpHandler;

    public TelemetryUploaderTests( ITestOutputHelper logger ) : base(
        logger,
        services =>
            services
        .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() { IsTelemetryEnabled = true } ) )
        .AddSingleton<IPlatformInfo>( new PlatformInfo( services.ServiceProvider, null ) )
        .AddSingleton<IHttpClientFactory>( new TestHttpClientFactory( new TelemetryTestsPutMessageHandler( services.ServiceProvider, _feedbackDirectory ) ) )
        .AddTelemetryServices() )
    { 
        this.FileSystem.CreateDirectory( _feedbackDirectory );
        var httpClientFactory = (TestHttpClientFactory) this.ServiceProvider.GetRequiredBackstageService<IHttpClientFactory>();
        this._httpHandler = (TelemetryTestsPutMessageHandler) httpClientFactory.Handler;

        new WelcomeService( this.ServiceProvider ).ExecuteFirstStartSetup( false, false );
    }

    private async Task AssertUploadedAsync(bool uploadedFileExpected)
    {
        var uploader = this.ServiceProvider.GetRequiredBackstageService<ITelemetryUploader>();
        await uploader.UploadAsync();

        var processedRequests = this._httpHandler.ProcessedRequests;
        var uploadedFiles = this.FileSystem.EnumerateFiles( _feedbackDirectory, "*.psf" );

        if ( uploadedFileExpected )
        {
            Assert.Single( processedRequests );
            Assert.Single( uploadedFiles );
        }
        else
        {
            Assert.Empty( processedRequests );
            Assert.Empty( uploadedFiles );
        }
    }

    [Fact]
    public async Task ServiceNotCalledWhenNothingToUpload()
    {
        await this.AssertUploadedAsync( false );
    }

    [Fact]
    public async Task UsageIsUploaded()
    {
        var usageReporter = this.ServiceProvider.GetRequiredBackstageService<IUsageReporter>();
        usageReporter.StartSession( "TestUsage" );
        usageReporter.StopSession();

        await this.AssertUploadedAsync( true );
    }

    [Fact]
    public async Task ExceptionsAreUploaded()
    {
        var exceptionsReporter = this.ServiceProvider.GetRequiredBackstageService<IExceptionReporter>();
        exceptionsReporter.ReportException( new InvalidOperationException( "Test Exception" ) );

        await this.AssertUploadedAsync( true );
    }

    [Fact]
    public async Task PerformanceProblemsAreUploaded()
    {
        var exceptionsReporter = this.ServiceProvider.GetRequiredBackstageService<IExceptionReporter>();
        exceptionsReporter.ReportException( new InvalidOperationException( "Test Performance Problem" ), ExceptionReportingKind.PerformanceProblem );

        await this.AssertUploadedAsync( true );
    }
}

#endif