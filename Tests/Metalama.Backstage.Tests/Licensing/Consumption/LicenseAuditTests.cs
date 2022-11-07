// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Audit;
using Metalama.Backstage.Licensing.Tests.Licensing.Licenses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Consumption;

public class LicenseAuditTests : LicenseConsumptionManagerTestsBase
{
    public LicenseAuditTests( ITestOutputHelper logger ) : base(
        logger,
        serviceBuilder => serviceBuilder.AddSingleton<ILicenseAuditManager>( new LicenseAuditManager( serviceBuilder.ServiceProvider ) ) ) { }

    private TestLicense CreateAndConsumeLicense( string licenseKey )
    {
        var license = this.CreateLicense( licenseKey );
        var manager = this.CreateConsumptionManager( license );
        Assert.True( manager.CanConsume( LicenseRequirement.Free ) );

        return license;
    }

    private string[] GetReports()
    {
        static bool IsAuditReport( string path ) => Path.GetFileName( path ) == "LicenseAudit-0.log";

        var reportText = this.FileSystem.ReadAllText( this.FileSystem.Mock.AllFiles.Single( IsAuditReport ) );
        var reports = reportText.Split( Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries );

        return reports;
    }

    [Theory]
    [InlineData( TestLicenses.MetalamaUltimateBusiness )]
    [InlineData( TestLicenses.MetalamaUltimateBusinessNotAuditable )]
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
        var licenseKeys = new List<string> { TestLicenses.MetalamaUltimateBusiness, TestLicenses.MetalamaProfessionalBusiness };

        licenseKeys.ForEach( l => this.CreateAndConsumeLicense( l ) );

        var reports = this.GetReports();

        Assert.Equal( licenseKeys.Count, reports.Length );

        for ( var i = 0; i < licenseKeys.Count; i++ )
        {
            Assert.Contains( licenseKeys[i], reports[i], StringComparison.OrdinalIgnoreCase );
        }
    }

    [Fact]
    public void LicenseAuditReportsSameLicenseKeyDaily()
    {
        const string licenseKey = TestLicenses.MetalamaUltimateBusiness;

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