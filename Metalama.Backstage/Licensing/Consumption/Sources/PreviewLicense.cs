// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Backstage.Licensing.Consumption.Sources;

/// <summary>
/// A license source that allows to use the product up to 60 days after the build date of a pre-release version.
/// </summary>
internal sealed class PreviewLicense : ILicenseSource, ILicense
{
    internal const int PreviewLicensePeriod = 60;
    internal const int WarningPeriod = 15;
    private readonly IDateTimeProvider _time;
    private readonly IApplicationInfo _applicationInfo;
    private bool _messageReported;

    public PreviewLicense( IServiceProvider serviceProvider )
    {
        this._time = serviceProvider.GetRequiredService<IDateTimeProvider>();
        this._applicationInfo = serviceProvider.GetRequiredService<IApplicationInfo>();
    }

    public IEnumerable<ILicense> GetLicenses( Action<LicensingMessage> reportMessage )
    {
        if ( !this._applicationInfo.IsPrerelease )
        {
            return Array.Empty<ILicense>();
        }

        var age = (int) (this._time.Now - this._applicationInfo.BuildDate).TotalDays;

        if ( age > PreviewLicensePeriod )
        {
            reportMessage(
                new LicensingMessage(
                    $"This preview build of Metalama has expired on {this._applicationInfo.BuildDate.AddDays( PreviewLicensePeriod )}. To continue using Metalama, update it to a newer preview, register a license key, or switch to Metalama Essentials.",
                    true ) );

            this._messageReported = true;

            return Array.Empty<ILicense>();
        }

        if ( !this._messageReported && age > (PreviewLicensePeriod - WarningPeriod) )
        {
            reportMessage(
                new LicensingMessage(
                    $"Your free preview license of Metalama will expire on {this._applicationInfo.BuildDate.AddDays( PreviewLicensePeriod )}. Please update Metalama to a newer preview, register a license key, or switch to Metalama Essentials." ) );

            this._messageReported = true;
        }

        return new ILicense[] { this };
    }

    bool ILicense.TryGetLicenseConsumptionData( [MaybeNullWhen( false )] out LicenseConsumptionData licenseConsumptionData )
    {
        licenseConsumptionData = new LicenseConsumptionData(
            LicensedProduct.MetalamaUltimate,
            LicenseType.Preview,
            LicensedFeatures.All,
            null,
            "Preview License",
            new Version( 0, 0 ) );

        return true;
    }

    bool ILicense.TryGetLicenseRegistrationData( [MaybeNullWhen( false )] out LicenseRegistrationData licenseRegistrationData )
    {
        licenseRegistrationData = null;

        return false;
    }
}