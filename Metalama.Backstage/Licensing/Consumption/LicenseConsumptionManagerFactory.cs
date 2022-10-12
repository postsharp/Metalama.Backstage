// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption.Sources;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption;

internal static class LicenseConsumptionManagerFactory
{
    public static ILicenseConsumptionManager Create(
        IServiceProvider serviceProvider,
        LicensingInitializationOptions options )
    {
        var licenseSources = new List<ILicenseSource>();

        // We ignore the unattended license if we have a license key in the csproj because it allows to test licensing on build servers.
        // If we choose to take into account the unattended license regardless, we would need another way to disable the unattended license.
        if ( !options.IgnoreUnattendedProcessLicense && string.IsNullOrWhiteSpace( options.ProjectLicense ) )
        {
            licenseSources.Add( new UnattendedLicenseSource( serviceProvider ) );
        }

        if ( !options.IgnoreUserProfileLicenses )
        {
            licenseSources.Add( new UserProfileLicenseSource( serviceProvider ) );
        }

        if ( !string.IsNullOrWhiteSpace( options.ProjectLicense ) )
        {
            licenseSources.Add( new ExplicitLicenseSource( options.ProjectLicense!, serviceProvider ) );
        }

        if ( !options.IgnoreUserProfileLicenses )
        {
            // Must be added last.
            licenseSources.Add( new PreviewLicenseSource( serviceProvider ) );
        }

        return new LicenseConsumptionManager( serviceProvider, licenseSources );
    }
}