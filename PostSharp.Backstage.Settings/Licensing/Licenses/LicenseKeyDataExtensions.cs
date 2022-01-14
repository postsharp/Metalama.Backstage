// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Registration;
using System;
using System.Globalization;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal static class LicenseKeyDataExtensions
    {
        private static LicenseType GetLicenseType(this LicenseKeyData licenseKeyData)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            if ( licenseKeyData.Product == LicensedProduct.PostSharp30 && licenseKeyData.LicenseType == LicenseType.Professional )
#pragma warning restore CS0618 // Type or member is obsolete
            {
                return LicenseType.PerUser;
            }
            else
            {
                return licenseKeyData.LicenseType;
            }
        }

        private static string GetProductName( this LicenseKeyData licenseKeyData )
            => licenseKeyData.Product switch
            {
                LicensedProduct.Framework => "PostSharp Framework",
                LicensedProduct.Ultimate => licenseKeyData.LicenseType == LicenseType.Community ? "PostSharp Community" : "PostSharp Ultimate",
                LicensedProduct.DiagnosticsLibrary => "PostSharp Logging",
                LicensedProduct.ModelLibrary => "PostSharp MVVM",
                LicensedProduct.ThreadingLibrary => "PostSharp Threading",
                LicensedProduct.CachingLibrary => "PostSharp Caching",
                LicensedProduct.MetalamaUltimate => "Metalama Ultimate",
                LicensedProduct.MetalamaProfessional => "Metalama Professional",
                _ => string.Format( CultureInfo.InvariantCulture, "Unknown Product ({0})", licenseKeyData.Product )
            };

        private static Version GetMinPostSharpVersion( this LicenseKeyData licenseKeyData )
        {
            // This logic is for PostSharp versions before 6.9.3.
            // The later versions are forward compatible without the need of updating of this logic.
            // Products not based on PostSharp (e.g. Metalama) don't need this logic at all.

            if ( licenseKeyData.MinPostSharpVersion != null )
            {
                return licenseKeyData.MinPostSharpVersion;
            }
            else if ( licenseKeyData.LicenseType == LicenseType.PerUsage || licenseKeyData.Product == LicensedProduct.CachingLibrary )
            {
                return new( 6, 6, 0 );
            }
#pragma warning disable 618
            else if ( licenseKeyData.Product == LicensedProduct.PostSharp20 )
            {
                return new( 2, 0, 0 );
            }
            else if ( (licenseKeyData.Product == LicensedProduct.Ultimate || licenseKeyData.Product == LicensedProduct.Framework)
                      && licenseKeyData.LicenseType == LicenseType.Enterprise )
            {
                return new( 5, 0, 22 );
            }
#pragma warning restore 618
            else if ( licenseKeyData.LicenseServerEligible != null )
            {
                return new( 5, 0, 22 );
            }
            else
            {
                return new( 3, 0, 0 );
            }
        }

        public static LicenseConsumptionData ToConsumptionData( this LicenseKeyData licenseKeyData )
        {
            var licenseType = licenseKeyData.GetLicenseType();

            var product = licenseKeyData.Product;

#pragma warning disable CS0618 // Type or member is obsolete
            if ( product == LicensedProduct.PostSharp30 )
            {
                product = licenseType == LicenseType.Professional ? LicensedProduct.Framework : LicensedProduct.Ultimate;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            var licensedFeatures = product switch
               {
                   LicensedProduct.Ultimate when licenseType != LicenseType.Community => LicensedProductFeatures.Ultimate,
                   LicensedProduct.Framework => LicensedProductFeatures.Framework,
                   LicensedProduct.ModelLibrary => LicensedProductFeatures.Mvvm,
                   LicensedProduct.ThreadingLibrary => LicensedProductFeatures.Threading,
                   LicensedProduct.DiagnosticsLibrary => LicensedProductFeatures.Logging,
                   LicensedProduct.CachingLibrary => LicensedProductFeatures.Caching,
                   LicensedProduct.MetalamaProfessional => LicensedProductFeatures.Metalama,
                   LicensedProduct.MetalamaUltimate => LicensedProductFeatures.Metalama,
                   _ => LicensedProductFeatures.Community
               };

            LicenseConsumptionData data = new(
                product,
                licenseType,
                licensedFeatures,
                licenseKeyData.Namespace,
                $"{licenseKeyData.GetProductName()} {licenseKeyData.LicenseType.GetLicenseTypeName()} ID {licenseKeyData.LicenseUniqueId}",
                licenseKeyData.GetMinPostSharpVersion() );

            return data;
        }

        public static LicenseRegistrationData ToRegistrationData( this LicenseKeyData licenseKeyData )
        {
            var description = $"{licenseKeyData.GetProductName()} ({licenseKeyData.LicenseType.GetLicenseTypeName()})";

            bool licenseServerEligible;

            if ( licenseKeyData.LicenseServerEligible.HasValue )
            {
                licenseServerEligible = licenseKeyData.LicenseServerEligible.Value;
            }
            else if ( licenseKeyData.LicenseType == LicenseType.PerUsage )
            {
                licenseServerEligible = false;
            }
            else
            {
                const int lastLicenseIdBefore50Rtm = 100802;
                licenseServerEligible = licenseKeyData.LicenseId > 0 && licenseKeyData.LicenseId <= lastLicenseIdBefore50Rtm;
            }

            var auditable = licenseKeyData.LicenseType switch
            {
#pragma warning disable CS0618 // Type or member is obsolete
                LicenseType.Site or LicenseType.Global or LicenseType.Anonymous => false,
#pragma warning restore CS0618 // Type or member is obsolete
                LicenseType.Evaluation => true, // We want to audit evaluation licenses so we know how people are using the product during evaluation.
                _ => licenseKeyData.Auditable ?? true
            };

            LicenseRegistrationData data = new(
                    licenseKeyData.LicenseUniqueId,
                    licenseKeyData.LicenseGuid != null,
                    licenseKeyData.LicenseGuid == null ? licenseKeyData.LicenseId : null,
                    licenseKeyData.Licensee,
                    description,
                    licenseKeyData.GetLicenseType(),
                    licenseKeyData.ValidFrom,
                    licenseKeyData.ValidTo,
                    !licenseKeyData.ValidTo.HasValue,
                    licenseKeyData.SubscriptionEndDate,
                    auditable,
                    licenseServerEligible,
                    licenseKeyData.GetMinPostSharpVersion() );

            return data;
        }
    }
}