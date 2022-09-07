// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Registration;
using System;
using System.Globalization;

namespace Metalama.Backstage.Licensing.Licenses
{
    /// <summary>
    /// Provides extension methods for processing license key data for license consumption, registration and audit.
    /// </summary>
    public static class LicenseKeyDataExtensions
    {
        /// <summary>
        /// If the <paramref name="licenseKeyData"/> contains an obsolete license type, it gets transformed to a respective non-obsolete one.
        /// Otherwise, the same license type is returned.
        /// </summary>
        private static LicenseType TransformObsoleteLicenseType( this LicenseKeyData licenseKeyData )
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

        private static LicensedProduct TransformObsoleteProduct( this LicenseKeyData licenseKeyData )
        {
            var product = licenseKeyData.Product;

#pragma warning disable CS0618 // Type or member is obsolete
            if ( product == LicensedProduct.PostSharp30 )
            {
                product = licenseKeyData.LicenseType == LicenseType.Professional ? LicensedProduct.PostSharpFramework : LicensedProduct.PostSharpUltimate;
            }
#pragma warning restore CS0618 // Type or member is obsolete

            return product;
        }

        private static string GetProductName( this LicenseKeyData licenseKeyData )
            => licenseKeyData.Product switch
            {
                LicensedProduct.PostSharpFramework => "PostSharp Framework",
                LicensedProduct.PostSharpUltimate => licenseKeyData.LicenseType == LicenseType.Essentials ? "PostSharp Essentials" : "PostSharp Ultimate",
                LicensedProduct.PostSharpLoggingLibrary => "PostSharp Logging",
                LicensedProduct.PostSharpMvvmLibrary => "PostSharp MVVM",
                LicensedProduct.PostSharpThreadingLibrary => "PostSharp Threading",
                LicensedProduct.PostSharpCachingLibrary => "PostSharp Caching",
                LicensedProduct.MetalamaUltimate => "Metalama Ultimate",
                LicensedProduct.MetalamaProfessional => "Metalama Professional",
                LicensedProduct.MetalamaStarter => "Metalama Starter",
                LicensedProduct.MetalamaFree => "Metalama Free",
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
            else if ( licenseKeyData.LicenseType == LicenseType.PerUsage || licenseKeyData.Product == LicensedProduct.PostSharpCachingLibrary )
            {
                return new Version( 6, 6, 0 );
            }
#pragma warning disable 618
            else if ( licenseKeyData.Product == LicensedProduct.PostSharp20 )
            {
                return new Version( 2, 0, 0 );
            }
            else if ( (licenseKeyData.Product == LicensedProduct.PostSharpUltimate || licenseKeyData.Product == LicensedProduct.PostSharpFramework)
                      && licenseKeyData.LicenseType == LicenseType.Enterprise )
            {
                return new Version( 5, 0, 22 );
            }
#pragma warning restore 618
            else if ( licenseKeyData.LicenseServerEligible != null )
            {
                return new Version( 5, 0, 22 );
            }
            else
            {
                return new Version( 3, 0, 0 );
            }
        }

        /// <summary>
        /// Creates a new object of <see cref="LicenseConsumptionData"/> based on the given <see cref="LicenseKeyData"/>.
        /// </summary>
        public static LicenseConsumptionData ToConsumptionData( this LicenseKeyData licenseKeyData )
        {
            var licenseType = licenseKeyData.TransformObsoleteLicenseType();
            var product = licenseKeyData.TransformObsoleteProduct();
            var isRedistributable = licenseType == LicenseType.OpenSourceRedistribution || licenseType == LicenseType.CommercialRedistribution;

            LicenseConsumptionData data = new(
                product,
                licenseType,
                licenseKeyData.Namespace,
                $"{licenseKeyData.GetProductName()} {licenseKeyData.LicenseType.GetLicenseTypeName()} ID {licenseKeyData.LicenseUniqueId}",
                licenseKeyData.GetMinPostSharpVersion(),
                licenseKeyData.LicenseString,
                isRedistributable );

            return data;
        }

        /// <summary>
        /// Creates a new object of <see cref="LicenseRegistrationData"/> based on the given <see cref="LicenseKeyData"/>.
        /// </summary>
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
#pragma warning restore CS0618                  // Type or member is obsolete
                LicenseType.Evaluation => true, // We want to audit evaluation licenses so we know how people are using the product during evaluation.
                _ => licenseKeyData.Auditable ?? true
            };

            LicenseRegistrationData data = new(
                licenseKeyData.LicenseUniqueId,
                licenseKeyData.LicenseGuid != null,
                licenseKeyData.LicenseGuid == null ? licenseKeyData.LicenseId : null,
                licenseKeyData.Licensee,
                description,
                licenseKeyData.TransformObsoleteProduct(),
                licenseKeyData.TransformObsoleteLicenseType(),
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