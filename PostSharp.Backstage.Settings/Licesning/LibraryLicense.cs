// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace PostSharp.Backstage.Licensing
{
    [Serializable]
    public class LibraryLicense : License
    {
        public LibraryLicense()
        {
        }

        internal LibraryLicense( object licenseData )
            : base( licenseData )
        {
        }

        protected override bool RequiresSignature()
        {
            return true;
        }

        public override bool RequiresRevocationCheck()
        {
            return false;
        }

        public override bool RequiresWatermark()
        {
            return false;
        }


        public override LicensedPackages GetLicensedPackages()
        {
            switch ( this.Product )
            {
                case LicensedProduct.ModelLibrary:
                    return LicensedProductPackages.Mvvm;
                case LicensedProduct.ThreadingLibrary:
                    return LicensedProductPackages.Threading;
                case LicensedProduct.DiagnosticsLibrary:
                    return LicensedProductPackages.Logging;
                case LicensedProduct.CachingLibrary:
                    return LicensedProductPackages.Caching;
                default:
                    return base.GetLicensedPackages();
            }
        }

        [Obsolete( "Use GetLicensedPackages() instead" )]
        public override IEnumerable<KeyValuePair<LicensedProduct, long>> GetLicensedFeatures()
        {
            yield return new KeyValuePair<LicensedProduct, long>( this.Product, 1 );
            yield return new KeyValuePair<LicensedProduct, long>( LicensedProduct.PostSharp30, (long) (LicensedFeatures.BasicFeatures) );
        }

        public override bool Validate( byte[] publicKeyToken, out string errorDescription )
        {
            if ( !base.Validate( publicKeyToken, out errorDescription ) )
                return false;

            if ( this.SubscriptionEndDate.HasValue && this.SubscriptionEndDate.Value < UserSettings.BuildDate )
            {
                errorDescription = "The build of PostSharp.Patterns you are using has been released after the end of the maintenance subscription.";
                return false;
            }

            return true;
        }

        /// <inheritdoc cref="License"/>
        public override int GetGraceDaysOrDefault()
        {
            return 30;
        }

        public override string GetProductName( bool detailed = false )
        {
            string productName;
            switch ( this.Product )
            {
                case LicensedProduct.DiagnosticsLibrary:
                    productName = "PostSharp Logging";
                    break;

                case LicensedProduct.ModelLibrary:
                    productName = "PostSharp MVVM";
                    break;

                case LicensedProduct.ThreadingLibrary:
                    productName = "PostSharp Threading";
                    break;
                
                case LicensedProduct.CachingLibrary:
                    productName = "PostSharp Caching";
                    break;

                default:
                    productName = string.Format(CultureInfo.InvariantCulture, "Unknown Product ({0})", this.Product );
                    break;
            }

            return productName;
        }

        public override bool IsAudited()
        {
            return true;
        }
    }
}
