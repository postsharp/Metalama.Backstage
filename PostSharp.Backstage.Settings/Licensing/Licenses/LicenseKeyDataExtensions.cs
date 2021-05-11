// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal static class LicenseKeyDataExtensions
    {
        public static LicenseConsumptionData ToConsumptionData( this LicenseKeyData licenseKeyData )
        {
            LicenseConsumptionData data = new(
                licensedProduct: licenseKeyData.Product,
                licenseType: licenseKeyData.LicenseType,
                licensedFeatures: licenseKeyData.LicensedFeatures,
                licensedNamespace: licenseKeyData.Namespace,
                displayName: $"{licenseKeyData.ProductName} {licenseKeyData.LicenseType.GetLicenseTypeName()} ID {licenseKeyData.LicenseUniqueId}" );

            return data;
        }

        public static LicenseRegistrationData ToRegistrationData( this LicenseKeyData licenseKeyData )
        {
            var description = $"{licenseKeyData.ProductName} ({licenseKeyData.LicenseType.GetLicenseTypeName()})";

            LicenseRegistrationData data = new(
                uniqueId: licenseKeyData.LicenseUniqueId,
                licensee: licenseKeyData.Licensee,
                description: description,
                licenseType: licenseKeyData.LicenseType,
                validFrom: licenseKeyData.ValidFrom,
                validTo: licenseKeyData.ValidTo,
                perpetual: !licenseKeyData.ValidTo.HasValue,
                subscriptionEndDate: licenseKeyData.SubscriptionEndDate );

            return data;
        }
    }
}
