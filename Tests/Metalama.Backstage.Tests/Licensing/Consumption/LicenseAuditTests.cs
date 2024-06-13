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
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing.Consumption;

public class LicenseAuditTests : LicenseConsumptionManagerTestsBase
{
    private static readonly string _auditedLicenseKey = TestLicenseKeys.MetalamaUltimateBusiness;
    
    public LicenseAuditTests( ITestOutputHelper logger ) : base(
        logger,
        serviceBuilder => serviceBuilder
            .AddSingleton<ITelemetryUploader>( new NullTelemetryUploader() )
            .AddSingleton<IUsageReporter>( new NullUsageReporter() )
            .AddSingleton<ILicenseAuditManager>( new LicenseAuditManager( serviceBuilder.ServiceProvider ) ),
        isTelemetryEnabled: true ) { }

    private TestLicense CreateAndConsumeLicense( string licenseKey )
    {
        var license = this.CreateLicense( licenseKey );
        var manager = this.CreateConsumptionManager( license );
        Assert.True( manager.CanConsume( LicenseRequirement.Free ) );

        return license;
    }

    private string[] GetReports()
    {
        var files = this.FileSystem.Mock.AllFiles.Where( path => Path.GetFileName( path ).StartsWith( "LicenseAudit-", StringComparison.Ordinal ) );
        var reports = files.SelectMany( f => this.FileSystem.ReadAllLines( f ) ).ToArray();

        return reports;
    }

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness) )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateBusinessNotAuditable) )]
    public void LicenseIsAudited( string licenseKey )
    {
        var license = this.CreateAndConsumeLicense( licenseKey );

        license.ResetUsage();
        Assert.True( license.TryGetLicenseConsumptionData( out var licenseData, out var errorMessage ) );
        Assert.Null( errorMessage );

        if ( licenseData.IsAuditable )
        {
            var reports = this.GetReports();
            Assert.Single( reports );
            Assert.Contains( licenseKey, reports[0], StringComparison.OrdinalIgnoreCase );
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

        for ( var i = 0; i < licenseKeys.Count; i++ )
        {
            Assert.Contains( licenseKeys[i], reports[i], StringComparison.OrdinalIgnoreCase );
        }
    }

    private void AssertReportsCount( int expectedCount )
    {
        var reports = this.GetReports();
        Assert.Equal( expectedCount, reports.Length );
        Assert.All( reports, r => Assert.Contains( _auditedLicenseKey, r, StringComparison.OrdinalIgnoreCase ) );
    }

    private void ConsumeAndAssertReportsCount( int expectedCount )
    {
        this.CreateAndConsumeLicense( _auditedLicenseKey );
        this.AssertReportsCount( expectedCount );
    }
    
    [Fact]
    public void LicenseAuditReportsSameLicenseKeyDaily()
    {
        Assert.Empty( this.FileSystem.Mock.AllFiles );

        var now = new DateTime( 2022, 01, 01 );

        void ShiftTime( TimeSpan span )
        {
            now += span;
            this.Time.Set( now );
        }

        ShiftTime( TimeSpan.Zero );
        this.ConsumeAndAssertReportsCount( 1 );

        this.ConsumeAndAssertReportsCount( 1 );

        ShiftTime( TimeSpan.FromDays( 1 ) - TimeSpan.FromMilliseconds( 1 ) );
        this.ConsumeAndAssertReportsCount( 1 );

        ShiftTime( TimeSpan.FromMilliseconds( 1 ) );
        this.ConsumeAndAssertReportsCount( 2 );

        this.ConsumeAndAssertReportsCount( 2 );

        ShiftTime( TimeSpan.FromDays( 1 ) - TimeSpan.FromMilliseconds( 1 ) );
        this.ConsumeAndAssertReportsCount( 2 );

        ShiftTime( TimeSpan.FromMilliseconds( 1 ) );
        this.ConsumeAndAssertReportsCount( 3 );
    }
    
    [Fact]
    public void LicenseIsNotReportedReportedWhenTelemetryIsDisabled()
    {
        ((TestApplicationInfo) this.ServiceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication).IsTelemetryEnabled = false;
        this.ConsumeAndAssertReportsCount( 0 );
    }
    
    [Fact]
    public void LicenseIsReportedWhenOptOutEnvironmentVariableIsSet()
    {
        this.EnvironmentVariableProvider.Environment["METALAMA_TELEMETRY_OPT_OUT"] = "true";
        this.ConsumeAndAssertReportsCount( 1 );
    }

    [Fact]
    public void LicenseIsNotReportedForUnattendedBuild()
    {
        ((TestApplicationInfo) this.ServiceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication).IsUnattendedProcess = true;
        this.ConsumeAndAssertReportsCount( 0 );
    }
}