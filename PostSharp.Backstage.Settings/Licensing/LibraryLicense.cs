// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace PostSharp.Backstage.Licensing
{
    public class LibraryLicense : ProductLicense
    {
        public LibraryLicense( Version productVersion, DateTime productBuildDate )
            : base( productVersion, productBuildDate )
        {
        }

        internal LibraryLicense( object licenseData, Version productVersion, DateTime productBuildDate )
            : base( licenseData, productVersion, productBuildDate )
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


        public override LicensedFeatures GetLicensedFeatures()
        {
            switch ( this.Product )
            {
                case LicensedProduct.ModelLibrary:
                    return LicensedProductFeatures.Mvvm;
                case LicensedProduct.ThreadingLibrary:
                    return LicensedProductFeatures.Threading;
                case LicensedProduct.DiagnosticsLibrary:
                    return LicensedProductFeatures.Logging;
                case LicensedProduct.CachingLibrary:
                    return LicensedProductFeatures.Caching;
                default:
                    return base.GetLicensedFeatures();
            }
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
