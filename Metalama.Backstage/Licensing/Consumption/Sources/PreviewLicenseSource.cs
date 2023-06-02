// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using System;
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

    public ILicense? GetLicense( Action<LicensingMessage> reportMessage )
    {
        var latestPrereleaseComponent = this.GetLatestPrereleaseComponent();

        if ( latestPrereleaseComponent == null )
        {
            this._logger.Trace?.Log( "PreviewLicenseSource skipped: there is no pre-release component." );

            return null;
        }

        this._logger.Trace?.Log( $"The latest prerelease component for '{this._applicationInfo.Name}' application is '{latestPrereleaseComponent.Name}'." );

        var age = (int) (this._time.Now - latestPrereleaseComponent.BuildDate!.Value).TotalDays;

        if ( age > PreviewLicensePeriod )
        {
            this._logger.Trace?.Log( "PreviewLicenseSource failed: the pre-release build has expired." );

            reportMessage(
                new LicensingMessage(
                    $"Your preview build of {latestPrereleaseComponent.Name} {latestPrereleaseComponent.Version} has expired on {latestPrereleaseComponent.BuildDate!.Value.AddDays( PreviewLicensePeriod ):d}. To continue using {latestPrereleaseComponent.Name}, update it to a newer preview build, register a license key, or switch to Metalama Free. See https://postsharp.net/links/metalama-register-license for details.",
                    true ) );

            this._messageReported = true;

            return null;
        }

        this._logger.Trace?.Log( "PreviewLicenseSource: providing a license." );

        if ( !this._messageReported && age > PreviewLicensePeriod - WarningPeriod )
        {
            this._logger.Trace?.Log( "PreviewLicenseSource warning: the pre-release build is close to expiration." );

            reportMessage(
                new LicensingMessage(
                    $"Your preview build of {latestPrereleaseComponent.Name} {latestPrereleaseComponent.Version} will expire on {latestPrereleaseComponent.BuildDate!.Value.AddDays( PreviewLicensePeriod ):d}. Please update {latestPrereleaseComponent.Name} to a newer preview, register a license key, or switch to Metalama Free. See https://postsharp.net/links/metalama-register-license for details" ) );

            this._messageReported = true;
        }

        return this;
    }

    event Action? ILicenseSource.Changed { add { } remove { } }

    private IComponentInfo? GetLatestPrereleaseComponent()
    {
        IComponentInfo latestComponent = this._applicationInfo;

        foreach ( var component in this._applicationInfo.Components )
        {
            if ( !component.IsPreviewLicenseEligible() && component.BuildDate <= latestComponent.BuildDate )
            {
                latestComponent = component;
            }
        }

        return latestComponent.IsPreviewLicenseEligible() ? latestComponent : null;
    }

    bool ILicense.TryGetLicenseConsumptionData(
        [MaybeNullWhen( false )] out LicenseConsumptionData licenseConsumptionData,
        [MaybeNullWhen( true )] out string errorMessage )
    {
        licenseConsumptionData = new LicenseConsumptionData(
            LicensedProduct.MetalamaUltimate,
            LicenseType.Preview,
            null,
            "Preview License",
            new Version( 0, 0 ),
            null,
            false,
            false );

        errorMessage = null;

        return true;
    }

    bool ILicense.TryGetLicenseRegistrationData(
        [MaybeNullWhen( false )] out LicenseRegistrationData licenseRegistrationData,
        [MaybeNullWhen( true )] out string errorMessage )
        => throw new NotSupportedException( "Preview license source doesn't support license registration." );
}