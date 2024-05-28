﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Audit;
using Metalama.Backstage.Telemetry;
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

        services.AddSingleton( serviceProvider => new TelemetryLogger( serviceProvider ) );
        services.AddSingleton<ILicenseAuditManager>( serviceProvider => new LicenseAuditManager( serviceProvider ) );
        services.AddSingleton<TelemetryReportUploader>( serviceProvider => new TelemetryReportUploader( serviceProvider ) );
        services.AddSingleton( serviceProvider => new MatomoAuditUploader( serviceProvider ) );
        services.AddSingleton( serviceProvider => new BackstageServicesInitializer( serviceProvider ) );
    }

    private TestLicense CreateAndConsumeLicense( string licenseKey )
    {
        var license = this.CreateLicense( licenseKey );
        var consumer = this.CreateConsumptionService( license ).CreateConsumer();
        Assert.True( consumer.CanConsume( LicenseRequirement.Free ) );

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
    [InlineData( nameof(LicenseKeyProvider.MetalamaUltimateBusiness) )]
    [InlineData( nameof(LicenseKeyProvider.MetalamaUltimateBusinessNotAuditable) )]
    public void LicenseIsAudited( string licenseKeyName )
    {
        var licenseKey = LicenseKeyProvider.GetLicenseKey( licenseKeyName );
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
            var matomoRequestUri = matomoRequest.Request.RequestUri?.ToString();

            this.Logger.WriteLine( matomoRequestUri );

            Assert.Equal( HttpMethod.Get, matomoRequest.Request.Method );

            Assert.Equal(
                "https://postsharp.matomo.cloud/matomo.php?idsite=6&rec=1&_id=36579f554ac8899f&dimension1=MetalamaUltimate&dimension2=PerUser&dimension3=Metalama&dimension4=1.0&new_visit=1&rand=5cf58a1a689e1e0c",
                matomoRequestUri );

            // Second time in the same day.
            this.FileSystem.Reset();

            var secondLicense = this.CreateAndConsumeLicense( licenseKey );
            Assert.True( secondLicense.TryGetLicenseConsumptionData( out _, out _ ) );
            var secondReports = this.GetReports();
            Assert.Empty( secondReports );

            // Third time, one day later.
            this.FileSystem.Reset();
            this.HttpClientFactory.Reset();
            this.Time.AddTime( TimeSpan.FromDays( 1 ) );

            var thirdLicense = this.CreateAndConsumeLicense( licenseKey );
            Assert.True( thirdLicense.TryGetLicenseConsumptionData( out _, out _ ) );
            var thirdReports = this.GetReports();
            Assert.Single( thirdReports );

            var thirdMatomoRequest = Assert.Single(
                this.HttpClientFactory.ProcessedRequests.Where( r => r.Request.RequestUri?.Host == "postsharp.matomo.cloud" ) );

            var thirdMatomoRequestUri = thirdMatomoRequest.Request.RequestUri?.ToString();

            this.Logger.WriteLine( thirdMatomoRequestUri );

            Assert.Equal( HttpMethod.Get, thirdMatomoRequest.Request.Method );

            Assert.Equal(
                "https://postsharp.matomo.cloud/matomo.php?idsite=6&rec=1&_id=36579f554ac8899f&dimension1=MetalamaUltimate&dimension2=PerUser&dimension3=Metalama&dimension4=1.0&new_visit=0&rand=624e91464771d36f",
                thirdMatomoRequestUri );
        }
        else
        {
            Assert.Empty( this.FileSystem.Mock.AllFiles );
        }
    }

    [Fact]
    public void LicenseAuditReportsDistinctLicenseKeyWithNoDelay()
    {
        var licenseKeys = new List<string> { LicenseKeyProvider.MetalamaUltimateBusiness, LicenseKeyProvider.MetalamaProfessionalBusiness };

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
        var licenseKey = LicenseKeyProvider.MetalamaUltimateBusiness;

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

    [Fact]
    public void DeviceIdIsSerialized()
    {
        var guid = new Guid( "75c1ce19-e594-4bfe-ac39-e37b9dd62069" );
        var configuration = new TelemetryConfiguration { DeviceId = guid };
        var json = configuration.ToJson();
        Assert.Contains( guid.ToString(), json, StringComparison.Ordinal );
    }
}