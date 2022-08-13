// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Backstage.Licensing.Audit
{
    // TODO
    [Serializable]
    public struct ReportedLicense : IEquatable<ReportedLicense>
    {
        public string LicensedProduct { get; }

        public string LicenseType { get; }

        public ReportedLicense( string licensedProduct, string licenseType )
        {
            this.LicensedProduct = licensedProduct;
            this.LicenseType = licenseType;
        }

        public override string ToString()
        {
            return $"{this.LicensedProduct}-{this.LicenseType}";
        }

        public override bool Equals( object? obj )
        {
            if ( obj is ReportedLicense other )
            {
                return this.Equals( other );
            }

            return false;
        }

        public override int GetHashCode()
        {
#pragma warning disable CA1307
            return (this.LicensedProduct.GetHashCode() * 17) + this.LicenseType.GetHashCode();
#pragma warning restore CA1307
        }

        public static bool operator ==( ReportedLicense left, ReportedLicense right )
        {
            return left.Equals( right );
        }

        public static bool operator !=( ReportedLicense left, ReportedLicense right )
        {
            return !(left == right);
        }

        public bool Equals( ReportedLicense other )
        {
            return this.LicensedProduct.Equals( other.LicensedProduct, StringComparison.Ordinal ) &&
                   this.LicenseType.Equals( other.LicenseType, StringComparison.Ordinal );
        }
    }
}