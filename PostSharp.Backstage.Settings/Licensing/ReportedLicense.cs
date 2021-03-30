// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial source-available license. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing
{
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

        public override bool Equals(object obj)
        {
            if ( obj is ReportedLicense other )
                return this.Equals( other );

            return false;
        }

        public override int GetHashCode()
        {
            return ( this.LicensedProduct.GetHashCode() * 17 ) + this.LicenseType.GetHashCode();
        }

        public static bool operator ==(ReportedLicense left, ReportedLicense right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ReportedLicense left, ReportedLicense right)
        {
            return !(left == right);
        }

        public bool Equals(ReportedLicense other)
        {
            return this.LicensedProduct.Equals( other.LicensedProduct, StringComparison.InvariantCulture ) &&
                   this.LicenseType.Equals( other.LicenseType, StringComparison.InvariantCulture );
        }
    }
}