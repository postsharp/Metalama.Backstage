// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Metalama.Backstage.Tests.Licensing;

internal static class TestLicenseFactory
{
    public static string CreateMetalamaFreeLicense( IServiceProvider services )
    {
        var licenseFactory = new UnsignedLicenseFactory( services );
        var licenseKey = licenseFactory.CreateFreeLicense().LicenseKey;

        return licenseKey;
    }

    public static string CreateMetalamaEvaluationLicense( IServiceProvider services )
    {
        var licenseFactory = new UnsignedLicenseFactory( services );
        var licenseKey = licenseFactory.CreateEvaluationLicense().LicenseKey;

        return licenseKey;
    }

    public static PreviewLicenseSource CreatePreviewLicenseSource( bool isPrerelease, int daysAfterBuild )
    {
        var services = new ServiceCollection();
        var timeProvider = new TestDateTimeProvider();
        services.AddSingleton<IDateTimeProvider>( timeProvider );

        services.AddSingleton<IApplicationInfoProvider>(
            new ApplicationInfoProvider( new TestApplicationInfo( "Test", isPrerelease, "<version>", timeProvider.Now.AddDays( -daysAfterBuild ) ) ) );

        var serviceProvider = services.BuildServiceProvider();

        return new PreviewLicenseSource( serviceProvider );
    }

    public static UnattendedLicenseSource CreateUnattendedLicenseSource()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IApplicationInfoProvider>(
            new ApplicationInfoProvider( new TestApplicationInfo( "Test", false, "<version>", DateTime.Now ) { IsUnattendedProcess = true } ) );

        var servicesProvider = services.BuildServiceProvider();

        return new UnattendedLicenseSource( servicesProvider );
    }
}