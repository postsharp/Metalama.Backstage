// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Telemetry;

public class TelemetryUploaderTests : TestsBase
{
    public TelemetryUploaderTests( ITestOutputHelper logger ) : base(
        logger,
        services =>
            services.AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() { IsTelemetryEnabled = true } ) ) )
    { }

    [Fact]
    public void Test()
    {
        var uploader = this.ServiceProvider.GetRequiredBackstageService<ITelemetryUploader>();
        uploader.UploadAsync();
    }
}