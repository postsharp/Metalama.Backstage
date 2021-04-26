// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing
{
    public class LicenseData
    {
        public LicensedFeatures LicensedFeatures { get; }

        public string? LicensedNamespace { get; }

        public ReportedLicense ReportedLicense { get; }

        public string DisplayName { get; }

        public LicenseData( LicensedFeatures licensedFeatures, string? licensedNamespace, ReportedLicense reportedLicense, string displayName )
        {
            this.LicensedFeatures = licensedFeatures;
            this.LicensedNamespace = licensedNamespace;
            this.ReportedLicense = reportedLicense;
            this.DisplayName = displayName;
        }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}
