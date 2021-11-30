// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;
using System.Text;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal partial class LicenseKeyData
    {
        /// <summary>
        /// Since PostSharp 6.5.17, 6.8.10, and 6.9.3 the <see cref="MinPostSharpVersion" /> is no longer checked.
        /// For compatibility with previous version, all licenses with features introduced since these versions
        /// should have the <see cref="MinPostSharpVersion" />
        /// set to <see cref="MinPostSharpVersionValidationRemovedPostSharpVersion" />.
        /// </summary>
        public static readonly Version MinPostSharpVersionValidationRemovedPostSharpVersion = new( 6, 9, 3 );

        public bool RequiresSignature
        {
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (LicenseType == LicenseType.Anonymous)
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    return false;
                }

                if (LicenseId == 0 && ( LicenseType == LicenseType.Community || LicenseType == LicenseType.Evaluation ))
                {
                    return false;
                }

                return true;
            }
        }

        public string LicenseUniqueId => LicenseGuid.HasValue ? LicenseGuid.Value.ToString() : LicenseId.ToString( CultureInfo.InvariantCulture );

        // TODO in Caravela
        public bool RequiresWatermark => LicenseType == LicenseType.Evaluation || LicenseType == LicenseType.Academic;

        /// <summary>
        /// Gets the licensed features provided by this license.
        /// </summary>
        public LicensedFeatures LicensedFeatures
            => Product switch
            {
                LicensedProduct.Ultimate when LicenseType != LicenseType.Community => LicensedProductFeatures.Ultimate,
                LicensedProduct.Framework => LicensedProductFeatures.Framework,
                LicensedProduct.ModelLibrary => LicensedProductFeatures.Mvvm,
                LicensedProduct.ThreadingLibrary => LicensedProductFeatures.Threading,
                LicensedProduct.DiagnosticsLibrary => LicensedProductFeatures.Logging,
                LicensedProduct.CachingLibrary => LicensedProductFeatures.Caching,
                LicensedProduct.Caravela => LicensedProductFeatures.Caravela,
                _ => LicensedProductFeatures.Community
            };

        /// <summary>
        /// Gets a value indicating whether the license is limited by a namespace.
        /// </summary>
        public bool IsLimitedByNamespace => !string.IsNullOrEmpty( Namespace );

        public string ProductName
            => Product switch
            {
                LicensedProduct.Framework => "PostSharp Framework",
                LicensedProduct.Ultimate => LicenseType == LicenseType.Community ? "PostSharp Community" : "PostSharp Ultimate",
                LicensedProduct.DiagnosticsLibrary => "PostSharp Logging",
                LicensedProduct.ModelLibrary => "PostSharp MVVM",
                LicensedProduct.ThreadingLibrary => "PostSharp Threading",
                LicensedProduct.CachingLibrary => "PostSharp Caching",
                LicensedProduct.Caravela => "PostSharp Caravela",
                _ => string.Format( CultureInfo.InvariantCulture, "Unknown Product ({0})", Product )
            };

        public LicenseKeyData()
        {
            Version = 2;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendFormat(
                CultureInfo.InvariantCulture,
                "Version={0}, LicenseId={1}, LicenseType={2}, Product={3}",
                Version,
                LicenseId,
                LicenseType,
                Product );

            foreach (var licenseField in _fields)
            {
                stringBuilder.AppendFormat( CultureInfo.InvariantCulture, ", {0}={1}", licenseField.Key, licenseField.Value );
            }

            return stringBuilder.ToString();
        }
    }
}