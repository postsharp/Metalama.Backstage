// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Telemetry;

public class UsageReporterTests : TestsBase
{
    public UsageReporterTests( ITestOutputHelper logger ) : base( logger ) { }

    protected override void ConfigureServices( ServiceProviderBuilder services )
    {
        services.AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() { IsTelemetryEnabled = true } ) );
    }

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
        Assert.StartsWith( "Usage-", Path.GetFileName( this.FileSystem.Mock.AllFiles.Single() ), StringComparison.Ordinal );
    }
}