// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Backstage.Licensing.Consumption.Sources;

internal class UnattendedLicenseSource : ILicenseSource, ILicense
{
    private readonly ILogger _logger;

    public UnattendedLicenseSource( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().Licensing();
    }

    public IEnumerable<ILicense> GetLicenses( Action<LicensingMessage> reportMessage )
    {
        // Determines whether the current process is unattended. If logging is enabled, we do not go through the cached property, so we capture the log.
        var isProcessUnattended = this._logger.Trace != null ? ProcessUtilities.IsCurrentProcessUnattendedNoCache( this._logger ) : ProcessUtilities.IsCurrentProcessUnattended;

        if ( isProcessUnattended )
        {
            this._logger.Trace?.Log( "Providing an unattended process license." );

            return new ILicense[] { this };
        }
        else
        {
            this._logger.Trace?.Log( "The process is attended. Not providing an unattended process license." );

            return Array.Empty<ILicense>();
        }
    }

    bool ILicense.TryGetLicenseConsumptionData( [MaybeNullWhen( false )] out LicenseConsumptionData licenseConsumptionData )
    {
        licenseConsumptionData = new LicenseConsumptionData(
            LicensedProduct.MetalamaUltimate,
            LicenseType.Unattended,
            LicensedFeatures.All,
            null,
            "Unattended Process License",
            new Version( 0, 0 ) );

        return true;
    }

    bool ILicense.TryGetLicenseRegistrationData( [MaybeNullWhen( false )] out LicenseRegistrationData licenseRegistrationData )
    {
        licenseRegistrationData = null;

        return false;
    }
}