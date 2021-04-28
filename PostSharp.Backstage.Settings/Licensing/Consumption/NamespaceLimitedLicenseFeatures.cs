// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing.Consumption
{
    internal class NamespaceLimitedLicenseFeatures
    {
        public NamespaceConstraint Constraint { get; }

        public LicensedFeatures Features { get; set; }

        public NamespaceLimitedLicenseFeatures( string @namespace, LicensedFeatures features = LicensedFeatures.None )
        {
            this.Constraint = new(@namespace);
            this.Features = features;
        }
    }
}
