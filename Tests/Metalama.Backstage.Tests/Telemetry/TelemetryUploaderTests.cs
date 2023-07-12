// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NET

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Telemetry;

public class TelemetryUploaderTests : TestsBase
{
    public TelemetryUploaderTests( ITestOutputHelper logger ) : base(
        logger,
        services =>
            services
        .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() { IsTelemetryEnabled = true } ) )
        .AddSingleton<IPlatformInfo>( new PlatformInfo( services.ServiceProvider, null ) )
        .AddSingleton<IHttpClientFactory>( new TestHttpClientFactory( new TelemetryTestsPutMessageHandler( services.ServiceProvider, "" ) ) )
        .AddTelemetryServices() )
    { }

    [Fact]
    public async Task UsageIsUploaded()
    {
        var usageReporter = this.ServiceProvider.GetRequiredBackstageService<IUsageReporter>();

        Assert.True( usageReporter.IsUsageReportingEnabled );
        usageReporter.StartSession( "TestUsage" );
        usageReporter.StopSession();

        var uploader = this.ServiceProvider.GetRequiredBackstageService<ITelemetryUploader>();
        await uploader.UploadAsync();
    }
}

#endif