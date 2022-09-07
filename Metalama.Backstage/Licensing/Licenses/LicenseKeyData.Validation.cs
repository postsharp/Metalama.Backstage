// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses.LicenseFields;
using System;
using System.Diagnostics.CodeAnalysis;
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

            if ( this._fields.Keys.Any(
                    i =>
                        i.IsMustUnderstand()
                        && !Enum.IsDefined( typeof( LicenseFieldIndex ), i ) ) )
            {
                errorDescription = "The license contains unknown must-understand fields.";

                return false;
            }

            if ( this.SubscriptionEndDate.HasValue )
            {
                if ( !applicationInfo.BuildDate.HasValue )
                {
                    throw new InvalidOperationException( $"Application '{applicationInfo.Name}' is missing build date information." );
                }

                IComponentInfo latestComponentRequiringSubscription = applicationInfo;

                foreach ( var component in applicationInfo.Components )
                {
                    if ( component.Company != "PostSharp Technologies" )
                    {
                        continue;
                    }

                    if ( !component.BuildDate.HasValue )
                    {
                        throw new InvalidOperationException( $"Application component '{component.Name}' is missing build date information." );
                    }

                    if ( latestComponentRequiringSubscription.BuildDate < component.BuildDate )
                    {
                        latestComponentRequiringSubscription = component;
                    }
                }

                if ( this.SubscriptionEndDate < latestComponentRequiringSubscription.BuildDate )
                {
                    errorDescription = $"The licensed product '{latestComponentRequiringSubscription.Name}' version {latestComponentRequiringSubscription.Version} has been released on {latestComponentRequiringSubscription.BuildDate:d}, but the license key {this.LicenseId} only allows you to use versions released before {this.SubscriptionEndDate:d}.";

                    return false;
                }
            }

            if ( this.IsRedistribution && !this.IsLimitedByNamespace )
            {
                errorDescription = "The license is a redistribution license, but is not limited by a namespace.";

                return false;
            }

            if ( !this.IsRedistribution && this.IsLimitedByNamespace )
            {
                errorDescription = "The license is limited by namespace, but it is not a redistribution license.";

                return false;
            }

            errorDescription = null;

            return true;
        }
    }
}