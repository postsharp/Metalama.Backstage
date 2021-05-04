// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Backstage.Licensing.Consumption
{
    public class LicenseConsumptionData
    {
        public LicensedFeatures LicensedFeatures { get; }

        public string? LicensedNamespace { get; }

        public LicensedProduct LicensedProduct { get; }

        public LicenseType LicenseType { get; }

        public string DisplayName { get; }

        public LicenseConsumptionData(
            LicensedProduct licensedProduct,
            LicenseType licenseType,
            LicensedFeatures licensedFeatures,
            string? licensedNamespace,
            string displayName )
        {
            this.LicensedProduct = licensedProduct;
            this.LicenseType = licenseType;
            this.LicensedFeatures = licensedFeatures;
            this.LicensedNamespace = licensedNamespace;
            this.DisplayName = displayName;
        }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
