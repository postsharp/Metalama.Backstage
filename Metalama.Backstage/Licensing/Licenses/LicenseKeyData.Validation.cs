// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses.LicenseFields;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace Metalama.Backstage.Licensing.Licenses
{
    public partial class LicenseKeyData
    {
        public bool Validate(
            byte[]? publicKeyToken,
            IDateTimeProvider dateTimeProvider,
            IApplicationInfo applicationInfo,
            [MaybeNullWhen( true )] out string errorDescription )
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

            if ( !Enum.IsDefined( typeof(LicenseType), this.LicenseType ) )
            {
                errorDescription = "The license type is not known.";

                return false;
            }

            if ( !Enum.IsDefined( typeof(LicensedProduct), this.Product ) )
            {
                errorDescription = "The licensed product is not known.";

                return false;
            }

            if ( this._fields.Keys.Any(
                    i =>
                        i.IsMustUnderstand()
                        && !Enum.IsDefined( typeof(LicenseFieldIndex), i ) ) )
            {
                errorDescription = "The license contains unknown must-understand fields.";

                return false;
            }

            if ( this.SubscriptionEndDate.HasValue && this.SubscriptionEndDate.Value < applicationInfo.BuildDate )
            {
                errorDescription = string.Format(
                    CultureInfo.InvariantCulture,
                    "The licensed product version {0} has been released on {1:d}, but the license key {2} only allows you to use versions released before {3:d}.",
                    applicationInfo.Version,
                    applicationInfo.BuildDate,
                    this.LicenseId,
                    this.SubscriptionEndDate );

                return false;
            }

            errorDescription = null;

            return true;
        }
    }
}