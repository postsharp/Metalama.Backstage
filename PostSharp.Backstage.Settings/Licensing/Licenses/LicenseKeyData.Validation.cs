// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses.LicenseFields;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal partial class LicenseKeyData
    {
        public bool Validate( byte[]? publicKeyToken, IDateTimeProvider dateTimeProvider, DateTime productBuildDate, Version productVersion, [MaybeNullWhen(returnValue: true)] out string errorDescription )
        {
#pragma warning disable 618
            if ( this.LicenseType == LicenseType.Anonymous )
            {
                // Anonymous licenses are always valid but confer no right.
                errorDescription = null;
                return true;
            }
#pragma warning restore 618

            if ( !this.VerifySignature() )
            {
                errorDescription = "The license signature is invalid.";
                return false;
            }

            if ( this.ValidFrom.HasValue && this.ValidFrom > dateTimeProvider.Now )
            {
                errorDescription = "The license is not yet valid.";
                return false;
            }

            if ( this.ValidTo.HasValue && this.ValidTo < dateTimeProvider.Now )
            {
                errorDescription = "The license is not valid any more.";
                return false;
            }

            if ( this.PublicKeyToken != null )
            {
                if ( publicKeyToken == null )
                {
                    errorDescription = "The assembly is missing a public key token.";
                    return false;
                }

                if ( !ComparePublicKeyToken( publicKeyToken, this.PublicKeyToken ) )
                {
                    errorDescription = "The public key token of the assembly does not match the license.";
                    return false;
                }
            }

            if ( !Enum.IsDefined( typeof( LicenseType ), this.LicenseType ) )
            {
                errorDescription = "The license type is not known.";
                return false;
            }

            if ( !Enum.IsDefined( typeof( LicensedProduct ), this.Product ) )
            {
                errorDescription = "The licensed product is not known.";
                return false;
            }

            if ( this._fields.Keys.Any( i =>
              i.IsMustUnderstand()
              && !Enum.IsDefined( typeof( LicenseFieldIndex ), i ) ) )
            {
                errorDescription = "The license contains unknown must-understand fields.";
                return false;
            }

            if ( this.SubscriptionEndDate.HasValue && this.SubscriptionEndDate.Value < productBuildDate )
            {
                errorDescription = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1} has been released on {2:d}, but the license key {3} only allows you to use versions released before {4:d}.",
                    this.ProductName,
                    productVersion,
                    productBuildDate,
                    this.LicenseId,
                    this.SubscriptionEndDate );

                return false;
            }

            errorDescription = null;
            return true;
        }
    }
}
