// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Backstage.Licensing.Consumption.Sources;

/// <summary>
/// A license source that allows to use the product up to 45 days after the build date of a pre-release version.
/// </summary>
internal sealed class PreviewLicenseSource : ILicenseSource, ILicense
{
    internal const int PreviewLicensePeriod = 45;
    internal const int WarningPeriod = 7;
    private readonly IDateTimeProvider _time;
    private readonly IApplicationInfo _applicationInfo;
    private readonly ILogger _logger;

    private bool _messageReported;

    public PreviewLicenseSource( IServiceProvider serviceProvider )
    {
        this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
        this._logger = serviceProvider.GetLoggerFactory().Licensing();
    }

    public IEnumerable<ILicense> GetLicenses( Action<LicensingMessage> reportMessage )
    {
        if ( !this._applicationInfo.IsPrerelease )
        {
            this._logger.Trace?.Log( "PreviewLicenseSource skipped: the build is not pre-release." );

            return Array.Empty<ILicense>();
        }

        var age = (int) (this._time.Now - this._applicationInfo.BuildDate).TotalDays;

        if ( age > PreviewLicensePeriod )
        {
            this._logger.Trace?.Log( "PreviewLicenseSource failed: the pre-release build has expired." );

            reportMessage(
                new LicensingMessage(
                    $"Your preview license for this build of {this._applicationInfo.Name} {this._applicationInfo.Version} has expired on {this._applicationInfo.BuildDate.AddDays( PreviewLicensePeriod )}. To continue using {this._applicationInfo.Name}, update it to a newer preview build, register a license key, or switch to Metalama Essentials.",
                    true ) );

            this._messageReported = true;

            return Array.Empty<ILicense>();
        }

        this._logger.Trace?.Log( "PreviewLicenseSource: providing a license." );

        if ( !this._messageReported && age > PreviewLicensePeriod - WarningPeriod )
        {
            reportMessage(
                new LicensingMessage(
                    $"Your preview license of {this._applicationInfo.Name} {this._applicationInfo.Version} will expire on {this._applicationInfo.BuildDate.AddDays( PreviewLicensePeriod )}. Please update {this._applicationInfo.Name} to a newer preview, register a license key, or switch to Metalama Essentials." ) );

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