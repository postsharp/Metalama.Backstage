// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Audit;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Tests.Licensing.Licenses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing.Consumption;

public class LicenseAuditTests : LicenseConsumptionManagerTestsBase
{
    public LicenseAuditTests( ITestOutputHelper logger ) : base( logger, isTelemetryEnabled: true )
    {
        // Make sure that the telemetry configuration is initialized so we get a stable DeviceId.
        this.ServiceProvider.GetRequiredBackstageService<BackstageServicesInitializer>().Initialize();
    }

    protected override void ConfigureServices( ServiceProviderBuilder services )
    {
        base.ConfigureServices( services );

        services.AddSingleton<ILicenseAuditManager>( serviceProvider => new LicenseAuditManager( serviceProvider ) );
        services.AddSingleton<TelemetryReportUploader>( serviceProvider => new TelemetryReportUploader( serviceProvider ) );
        services.AddSingleton( serviceProvider => new MatomoAuditUploader( serviceProvider ) );
        services.AddSingleton( serviceProvider => new BackstageServicesInitializer( serviceProvider ) );
    }

    private TestLicense CreateAndConsumeLicense( string licenseKey )
    {
        var license = this.CreateLicense( licenseKey );
        var manager = this.CreateConsumptionManager( license );
        Assert.True( manager.CanConsume( LicenseRequirement.Free ) );

        return license;
    }

    private string[] GetReports()
    {
        this.BackgroundTasks.WhenNoPendingTaskAsync().Wait();

        var files = this.FileSystem.Mock.AllFiles.Where( path => Path.GetFileName( path ).StartsWith( "LicenseAudit-", StringComparison.Ordinal ) );
        var reports = files.SelectMany( f => this.FileSystem.ReadAllLines( f ) ).ToArray();

        return reports;
    }

    [Theory]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness) )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateBusinessNotAuditable) )]
    public void LicenseIsAudited( string licenseKeyName )
    {
        var licenseKey = TestLicenseKeys.GetLicenseKey( licenseKeyName );
        var license = this.CreateAndConsumeLicense( licenseKey );

        license.ResetUsage();
        Assert.True( license.TryGetLicenseConsumptionData( out var licenseData, out var errorMessage ) );
        Assert.Null( errorMessage );

        if ( licenseData.IsAuditable )
        {
            var reports = this.GetReports();
            Assert.Single( reports );
            Assert.Contains( licenseKey, reports[0], StringComparison.OrdinalIgnoreCase );
            var matomoRequest = Assert.Single( this.HttpClientFactory.ProcessedRequests.Where( r => r.Request.RequestUri?.Host == "postsharp.matomo.cloud" ) );
            Assert.Equal( HttpMethod.Get, matomoRequest.Request.Method );

            Assert.Equal(
                "https://postsharp.matomo.cloud/matomo.php?idsite=6&rec=1&_id=36579f554ac8899f&dimension1=MetalamaUltimate&dimension2=PerUser&dimension3=LicensingTestApp&dimension4=1.0",
                matomoRequest.Request.RequestUri?.ToString() );
        }
        else
        {
            Assert.Empty( this.FileSystem.Mock.AllFiles );
        }
    }

    [Fact]
    public void LicenseAuditReportsDistinctLicenseKeyWithNoDelay()
    {
        var licenseKeys = new List<string> { TestLicenseKeys.MetalamaUltimateBusiness, TestLicenseKeys.MetalamaProfessionalBusiness };

        licenseKeys.ForEach( l => this.CreateAndConsumeLicense( l ) );

        var reports = this.GetReports();

        Assert.Equal( licenseKeys.Count, reports.Length );

        foreach ( var t in licenseKeys )
        {
#pragma warning disable CA1307
            Assert.Contains( reports, r => r.Contains( t ) );
#pragma warning restore CA1307
        }
    }

    [Fact]
    public void LicenseAuditReportsSameLicenseKeyDaily()
    {
        var licenseKey = TestLicenseKeys.MetalamaUltimateBusiness;

        void AssertReportsCount( int expectedCount )
        {
            var reports = this.GetReports();
            Assert.Equal( expectedCount, reports.Length );
            Assert.All( reports, r => Assert.Contains( licenseKey, r, StringComparison.OrdinalIgnoreCase ) );
        }

        void ConsumeAndAssertReportsCount( int expectedCount )
        {
            this.CreateAndConsumeLicense( licenseKey );
            AssertReportsCount( expectedCount );
        }

        Assert.Empty( this.FileSystem.Mock.AllFiles );

        var now = new DateTime( 2022, 01, 01 );

        void ShiftTime( TimeSpan span )
        {
            now += span;
            this.Time.Set( now );
        }

        ShiftTime( TimeSpan.Zero );
        ConsumeAndAssertReportsCount( 1 );

        ConsumeAndAssertReportsCount( 1 );

        ShiftTime( TimeSpan.FromDays( 1 ) - TimeSpan.FromMilliseconds( 1 ) );
        ConsumeAndAssertReportsCount( 1 );

        ShiftTime( TimeSpan.FromMilliseconds( 1 ) );
        ConsumeAndAssertReportsCount( 2 );

        ConsumeAndAssertReportsCount( 2 );

        ShiftTime( TimeSpan.FromDays( 1 ) - TimeSpan.FromMilliseconds( 1 ) );
        ConsumeAndAssertReportsCount( 2 );

        ShiftTime( TimeSpan.FromMilliseconds( 1 ) );
        ConsumeAndAssertReportsCount( 3 );
    }
}