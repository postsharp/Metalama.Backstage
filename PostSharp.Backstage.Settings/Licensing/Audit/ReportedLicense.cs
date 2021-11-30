// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing.Audit
{
    // TODO
    [Serializable]
    public struct ReportedLicense : IEquatable<ReportedLicense>
    {
        public string LicensedProduct { get; }

        public string LicenseType { get; }

        public ReportedLicense( string licensedProduct, string licenseType )
        {
            LicensedProduct = licensedProduct;
            LicenseType = licenseType;
        }

        public override string ToString()
        {
            return $"{LicensedProduct}-{LicenseType}";
        }

        public override bool Equals( object obj )
        {
            if (obj is ReportedLicense other)
            {
                return Equals( other );
            }

            return false;
        }

        public override int GetHashCode()
        {
            return LicensedProduct.GetHashCode() * 17 + LicenseType.GetHashCode();
        }

        public static bool operator ==( ReportedLicense left, ReportedLicense right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( ReportedLicense left, ReportedLicense right )
        {
            return !( left == right );
        }

        public bool Equals( ReportedLicense other )
        {
            return LicensedProduct.Equals( other.LicensedProduct, StringComparison.Ordinal ) &&
                   LicenseType.Equals( other.LicenseType, StringComparison.Ordinal );
        }
    }
}