// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Telemetry;

public class UsageReporterTests : TestsBase
{
    public UsageReporterTests( ITestOutputHelper logger ) : base(
        logger,
        services =>
            services.AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() { IsTelemetryEnabled = true } ) ) ) { }

    [Fact]
    public void Test()
    {
        var usageReporter = new UsageReporter( this.ServiceProvider );
        Assert.Null( usageReporter.Metrics );
        Assert.True( usageReporter.StartSession( "SessionKind" ) );
        Assert.NotNull( usageReporter.Metrics );
        Assert.NotEmpty( usageReporter.Metrics! );
        usageReporter.StopSession();

        Assert.Single( this.FileSystem.Mock.AllFiles );
        Assert.Equal( "Usage-0.log", Path.GetFileName( this.FileSystem.Mock.AllFiles.Single() ) );
    }
}