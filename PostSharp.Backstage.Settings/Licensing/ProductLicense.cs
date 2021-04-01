// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System;
using System.Globalization;

namespace PostSharp.Backstage.Licensing
{
    public abstract class ProductLicense : License
    {
        public Version ProductVersion { get; }

        public DateTime ProductBuildDate { get; }

        public ProductLicense( Version productVersion, DateTime productBuildDate )
            : base()
        {
            this.ProductVersion = productVersion;
            this.ProductBuildDate = productBuildDate;
        }

        internal ProductLicense( object licenseData, Version productVersion, DateTime productBuildDate )
            : base( licenseData )
        {
            this.ProductVersion = productVersion;
            this.ProductBuildDate = productBuildDate;
        }

        public override bool Validate( byte[] publicKeyToken, IDateTimeProvider dateTimeProvider, out string errorDescription )
        {
            if ( !base.Validate( publicKeyToken, dateTimeProvider, out errorDescription ) )
            {
                return false;
            }

            if ( this.SubscriptionEndDate.HasValue && this.SubscriptionEndDate.Value < this.ProductBuildDate )
            {
                errorDescription = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1} has been released on {2:d}, but the license key {3} only allows you to use versions released before {4:d}.",
                    this.GetProductName(), this.ProductVersion, this.ProductBuildDate, this.LicenseId, this.SubscriptionEndDate );
                return false;
            }

            return true;
        }
    }
}
