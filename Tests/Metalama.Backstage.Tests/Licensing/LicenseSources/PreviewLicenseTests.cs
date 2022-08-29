// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Backstage.Licensing.Tests.Licensing.LicenseSources;

public class PreviewLicenseTests
{
    private static (bool HasLicense, bool HasMessage, LicensingMessage? Message) RunTest( bool isPrerelease, int daysAfterBuild )
    {
        var previewLicense = TestLicenses.CreatePreviewLicenseSource( isPrerelease, daysAfterBuild );
        var messages = new List<LicensingMessage>();
        var licenses = previewLicense.GetLicenses( messages.Add );

        return (licenses.Any(), messages.Any(), messages.SingleOrDefault());
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( PreviewLicenseSource.PreviewLicensePeriod - PreviewLicenseSource.WarningPeriod )]
    [InlineData( PreviewLicenseSource.PreviewLicensePeriod + 1 )]
    [InlineData( PreviewLicenseSource.PreviewLicensePeriod + 100 )]
    public void NoLicenseInStableRelease( int daysAfterBuild )
    {
        var result = RunTest( false, daysAfterBuild );

        Assert.False( result.HasLicense );
        Assert.False( result.HasMessage );
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( PreviewLicenseSource.PreviewLicensePeriod - PreviewLicenseSource.WarningPeriod )]
    public void TimeBombIsSafeBeforeWarningPeriod( int daysAfterBuild )
    {
        var result = RunTest( true, daysAfterBuild );

        Assert.True( result.HasLicense );
        Assert.False( result.HasMessage );
    }

    [Theory]
    [InlineData( PreviewLicenseSource.PreviewLicensePeriod - PreviewLicenseSource.WarningPeriod + 1 )]
    [InlineData( PreviewLicenseSource.PreviewLicensePeriod )]
    public void TimeBombWarnsDuringWarningPeriod( int daysAfterBuild )
    {
        var result = RunTest( true, daysAfterBuild );

        Assert.True( result.HasLicense );
        Assert.True( result.HasMessage );
    }

    [Theory]
    [InlineData( PreviewLicenseSource.PreviewLicensePeriod + 1 )]
    [InlineData( PreviewLicenseSource.PreviewLicensePeriod + 100 )]
    public void TimeBombExplodesAfterPreviewLicensePeriod( int daysAfterBuild )
    {
        var result = RunTest( true, daysAfterBuild );

        Assert.False( result.HasLicense );
        Assert.True( result.HasMessage );
        Assert.True( result.Message!.IsError );
    }
}