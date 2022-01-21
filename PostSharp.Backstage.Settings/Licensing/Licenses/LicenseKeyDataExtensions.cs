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
                licenseKeyData.Product,
                licenseKeyData.LicenseType,
                licenseKeyData.LicensedFeatures,
                licenseKeyData.Namespace,
                $"{licenseKeyData.ProductName} {licenseKeyData.LicenseType.GetLicenseTypeName()} ID {licenseKeyData.LicenseUniqueId}" );

            return data;
        }

        public static LicenseRegistrationData ToRegistrationData( this LicenseKeyData licenseKeyData )
        {
            var description = $"{licenseKeyData.ProductName} ({licenseKeyData.LicenseType.GetLicenseTypeName()})";

            LicenseRegistrationData data = new(
                licenseKeyData.LicenseUniqueId,
                licenseKeyData.LicenseGuid != null,
                licenseKeyData.LicenseGuid == null ? licenseKeyData.LicenseId : null,
                licenseKeyData.Licensee,
                description,
                licenseKeyData.LicenseType,
                licenseKeyData.ValidFrom,
                licenseKeyData.ValidTo,
                !licenseKeyData.ValidTo.HasValue,
                licenseKeyData.SubscriptionEndDate,
                licenseKeyData.Auditable,
                licenseKeyData.LicenseServerEligible );

            return data;
        }
    }
}