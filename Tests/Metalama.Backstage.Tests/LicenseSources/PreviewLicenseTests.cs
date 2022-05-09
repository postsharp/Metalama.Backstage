// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Backstage.Licensing.Tests.LicenseSources;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

public class PreviewLicenseTests
{
    private static PreviewLicenseSource CreatePreviewLicense( bool isPrerelease, int daysAfterBuild )
    {
        var services = new ServiceCollection();
        var timeProvider = new TestDateTimeProvider();
        services.AddSingleton<IDateTimeProvider>( timeProvider );
        services.AddSingleton<IApplicationInfo>( new TestApplicationInfo( "Test", isPrerelease, "<version>", timeProvider.Now.AddDays( -daysAfterBuild ) ) );
        var serviceProvider = services.BuildServiceProvider();

        return new PreviewLicenseSource( serviceProvider );
    }

    private static (bool HasLicense, bool HasMessage, LicensingMessage? Message) RunTest( bool isPrerelease, int daysAfterBuild )
    {
        var previewLicense = CreatePreviewLicense( isPrerelease, daysAfterBuild );
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