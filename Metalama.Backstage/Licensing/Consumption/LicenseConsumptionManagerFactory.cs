// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption.Sources;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption;

internal static class LicenseConsumptionManagerFactory
{
    public static ILicenseConsumptionManager Create(
        IServiceProvider serviceProvider,
        bool considerUnattendedLicense = false,
        bool ignoreUserProfileLicenses = false,
        string? additionalLicense = null )
    {
        var licenseSources = new List<ILicenseSource>();

        if ( considerUnattendedLicense )
        {
            licenseSources.Add( new UnattendedLicenseSource( serviceProvider ) );
        }

        if ( !ignoreUserProfileLicenses )
        {
            licenseSources.Add( new UserProfileLicenseSource( serviceProvider ) );
        }

        if ( !string.IsNullOrWhiteSpace( additionalLicense ) )
        {
            licenseSources.Add( new ExplicitLicenseSource( additionalLicense!, serviceProvider ) );
        }

        if ( !ignoreUserProfileLicenses )
        {
            // Must be added last.
            licenseSources.Add( new PreviewLicenseSource( serviceProvider ) );
        }

        return new LicenseConsumptionManager( serviceProvider, licenseSources );
    }
}