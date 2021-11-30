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
        public bool Validate(
            byte[]? publicKeyToken,
            IDateTimeProvider dateTimeProvider,
            IApplicationInfo applicationInfo,
            [MaybeNullWhen( true )] out string errorDescription )
        {
#pragma warning disable 618
            if (LicenseType == LicenseType.Anonymous)
            {
                // Anonymous licenses are always valid but confer no right.
                errorDescription = null;

                return true;
            }
#pragma warning restore 618

            if (!VerifySignature())
            {
                errorDescription = "The license signature is invalid.";

                return false;
            }

            if (ValidFrom.HasValue && ValidFrom > dateTimeProvider.Now)
            {
                errorDescription = "The license is not yet valid.";

                return false;
            }

            if (ValidTo.HasValue && ValidTo < dateTimeProvider.Now)
            {
                errorDescription = "The license is not valid any more.";

                return false;
            }

            if (PublicKeyToken != null)
            {
                if (publicKeyToken == null)
                {
                    errorDescription = "The assembly is missing a public key token.";

                    return false;
                }

                if (!ComparePublicKeyToken( publicKeyToken, PublicKeyToken ))
                {
                    errorDescription = "The public key token of the assembly does not match the license.";

                    return false;
                }
            }

            if (!Enum.IsDefined( typeof(LicenseType), LicenseType ))
            {
                errorDescription = "The license type is not known.";

                return false;
            }

            if (!Enum.IsDefined( typeof(LicensedProduct), Product ))
            {
                errorDescription = "The licensed product is not known.";

                return false;
            }

            if (_fields.Keys.Any(
                i =>
                    i.IsMustUnderstand()
                    && !Enum.IsDefined( typeof(LicenseFieldIndex), i ) ))
            {
                errorDescription = "The license contains unknown must-understand fields.";

                return false;
            }

            if (SubscriptionEndDate.HasValue && SubscriptionEndDate.Value < applicationInfo.BuildDate)
            {
                errorDescription = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1} has been released on {2:d}, but the license key {3} only allows you to use versions released before {4:d}.",
                    ProductName,
                    applicationInfo.Version,
                    applicationInfo.BuildDate,
                    LicenseId,
                    SubscriptionEndDate );

                return false;
            }

            errorDescription = null;

            return true;
        }
    }
}