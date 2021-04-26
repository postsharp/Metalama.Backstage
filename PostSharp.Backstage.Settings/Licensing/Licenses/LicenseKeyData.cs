// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses.LicenseFields;
using System;
using System.Collections.Generic;
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
        public static readonly Version MinPostSharpVersionValidationRemovedPostSharpVersion = new Version( 6, 9, 3 );

        public bool RequiresSignature
        {
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if ( this.LicenseType == LicenseType.Anonymous )
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    return false;
                }

                if ( this.LicenseId == 0 && (this.LicenseType == LicenseType.Community || this.LicenseType == LicenseType.Evaluation) )
                {
                    return false;
                }

                return true;
            }
        }

        public bool RequiresRevocationCheck => this.LicenseType == LicenseType.OpenSourceRedistribution;

        // TODO in Caravela
        public bool RequiresWatermark => this.LicenseType == LicenseType.Evaluation || this.LicenseType == LicenseType.Academic;

        /// <summary>
        /// Gets the licensed features provided by this license.
        /// </summary>
        public LicensedFeatures LicensedFeatures
        {
            get
            {
                switch ( this.Product )
                {
                    case LicensedProduct.Ultimate when this.LicenseType != LicenseType.Community:
                        return LicensedProductFeatures.Ultimate;
                    case LicensedProduct.Framework:
                        return LicensedProductFeatures.Framework;

                    case LicensedProduct.ModelLibrary:
                        return LicensedProductFeatures.Mvvm;
                    case LicensedProduct.ThreadingLibrary:
                        return LicensedProductFeatures.Threading;
                    case LicensedProduct.DiagnosticsLibrary:
                        return LicensedProductFeatures.Logging;
                    case LicensedProduct.CachingLibrary:
                        return LicensedProductFeatures.Caching;

                    case LicensedProduct.Caravela:
                        return LicensedProductFeatures.Caravela;

                    default:
                        return LicensedProductFeatures.Community;
                }
            }
        }

        /// <summary>
        /// Gets <c>true</c> when the <see cref="Namespace"/> property is set, otherwise <c>false</c>.
        /// </summary>
        public bool IsLimitedByNamespace => !string.IsNullOrEmpty( this.Namespace );
    }
}
