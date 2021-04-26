// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing
{
    [Serializable]
    public class CoreLicense : ProductLicense
    {
        public CoreLicense( LicensedProduct licensedProduct, Version productVersion, DateTime productBuildDate )
            : base( productVersion, productBuildDate )
        {
            this.Product = licensedProduct;
        }

        internal CoreLicense( object licenseData, Version productVersion, DateTime productBuildDate )
            : base( licenseData, productVersion, productBuildDate )
        {
        }

        protected override bool RequiresSignature()
        {
#pragma warning disable 618
            return this.LicenseType != LicenseType.Anonymous &&
                   !(this.LicenseId == 0 && (this.LicenseType == LicenseType.Community || this.LicenseType == LicenseType.Evaluation));
#pragma warning restore 618
        }

        public override bool RequiresRevocationCheck() => this.LicenseType == LicenseType.OpenSourceRedistribution;

        public override bool RequiresWatermark() => this.LicenseType == LicenseType.Evaluation || this.LicenseType == LicenseType.Academic;

        public override bool IsAudited()
        {
#pragma warning disable 618
            switch ( this.LicenseType )
            {
                case LicenseType.Site:
                case LicenseType.Global:
                case LicenseType.Anonymous:
                    return false;

                case LicenseType.Evaluation:
                    // We want to audit evaluation licenses so we know how people are using the product during evaluation.
                    return true;

                default:
                    return this.Auditable.GetValueOrDefault( true );
            }
#pragma warning restore 618
        }

        public override bool Validate( byte[] publicKeyToken, IDateTimeProvider dateTimeProvider, out string errorDescription )
        {
#pragma warning disable 618
            if ( this.LicenseType == LicenseType.Anonymous )
            {
                // Anonymous licenses are always valid but confer no right.
                errorDescription = null;
                return true;
            }
#pragma warning restore 618

            if ( !base.Validate( publicKeyToken, dateTimeProvider, out errorDescription ) )
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc cref="License"/>
        public override int GetGraceDaysOrDefault() => 30;

        public override LicensedFeatures GetLicensedFeatures()
        {
            switch ( this.Product )
            {
                case LicensedProduct.Ultimate when this.LicenseType != LicenseType.Community:
                    return LicensedProductFeatures.Ultimate;
                case LicensedProduct.Framework:
                    return LicensedProductFeatures.Framework;
                default:
                    // Community Edition.
                    return base.GetLicensedFeatures();
            }
        }

        public override string GetProductName(bool detailed = false)
        {
            switch ( this.Product )
            {
                case LicensedProduct.Framework:
                    return "PostSharp Framework";
                case LicensedProduct.Ultimate:
                    return this.LicenseType == LicenseType.Community ? "PostSharp Community" : "PostSharp Ultimate";
                default:
                    return "PostSharp";
            }
        }
         
        public override bool IsEvaluationLicense() => this.LicenseType == LicenseType.Evaluation;

        public override bool IsUserLicense() => this.LicenseType.IsUserLicense();
    }

  

    [Serializable]
    internal class CoreUnattendedLicense : CoreLicense
    {
        public CoreUnattendedLicense( Version productVersion, DateTime productBuildDate )
            : base( LicensedProduct.Ultimate, productVersion, productBuildDate )
        {
            this.LicenseType = LicenseType.Unattended;
            this.UserNumber = 1;
        }

        protected override bool RequiresSignature() => false;

        public override bool IsAudited() => false;

        public override LicenseSource GetAllowedLicenseSources() => LicenseSource.Internal;

        public override LicensedFeatures GetLicensedFeatures() => LicensedProductFeatures.Unattended;

    }

    [Serializable]
    internal class CoreUnmodifiedLicense : CoreLicense
    {
        public CoreUnmodifiedLicense( Version productVersion, DateTime productBuildDate )
            : base( LicensedProduct.Ultimate, productVersion, productBuildDate )
        {
            this.LicenseType = LicenseType.Unmodified;
            this.UserNumber = 1;
        }

        protected override bool RequiresSignature() => false;

        public override bool IsAudited() => false;

        public override LicenseSource GetAllowedLicenseSources() => LicenseSource.Internal;

        public override LicensedFeatures GetLicensedFeatures() => LicensedFeatures.All;
    }
}
