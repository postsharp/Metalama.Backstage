// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;
using System.Text;

namespace PostSharp.Backstage.Licensing.Licenses
{
    /// <summary>
    /// Provides serialization, cryptography and validation for license keys.
    /// </summary>
    public partial class LicenseKeyData
    {
        /// <summary>
        /// Since PostSharp 6.5.17, 6.8.10, and 6.9.3 the <see cref="MinPostSharpVersion" /> is no longer checked.
        /// For compatibility with previous version, all licenses with features introduced since these versions
        /// should have the <see cref="MinPostSharpVersion" />
        /// set to <see cref="_minPostSharpVersionValidationRemovedPostSharpVersion" />.
        /// </summary>
        private static readonly Version _minPostSharpVersionValidationRemovedPostSharpVersion = new( 6, 9, 3 );

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

        public string LicenseUniqueId
            => this.LicenseGuid.HasValue ? this.LicenseGuid.Value.ToString() : this.LicenseId.ToString( CultureInfo.InvariantCulture );

        // TODO in Metalama
        public bool RequiresWatermark => this.LicenseType == LicenseType.Evaluation || this.LicenseType == LicenseType.Academic;

        /// <summary>
        /// Gets a value indicating whether the license is limited by a namespace.
        /// </summary>
        public bool IsLimitedByNamespace => !string.IsNullOrEmpty( this.Namespace );

        public LicenseKeyData()
        {
            this.Version = 2;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder stringBuilder = new();

            stringBuilder.AppendFormat(
                CultureInfo.InvariantCulture,
                "Version={0}, LicenseId={1}, LicenseType={2}, Product={3}",
                this.Version,
                this.LicenseId,
                this.LicenseType,
                this.Product );

            foreach ( var licenseField in this._fields )
            {
                stringBuilder.AppendFormat( CultureInfo.InvariantCulture, ", {0}={1}", licenseField.Key, licenseField.Value );
            }

            return stringBuilder.ToString();
        }
    }
}