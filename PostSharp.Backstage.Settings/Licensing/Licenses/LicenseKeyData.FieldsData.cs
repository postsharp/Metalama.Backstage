// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses.LicenseFields;
using System;

namespace PostSharp.Backstage.Licensing.Licenses
{
    public partial class LicenseKeyData
    {
        public string? LicenseString { get; private set; }

        /// <summary>
        /// Gets the license version.
        /// </summary>
        public byte Version { get; private set; }

        public Guid? LicenseGuid { get; set; }

        /// <summary>
        /// Gets or sets the type of license.
        /// </summary>
        public LicenseType LicenseType { get; set; }

        /// <summary>
        /// Gets or sets the licensed product.
        /// </summary>
        public LicensedProduct Product { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the current license.
        /// </summary>
        public int LicenseId { get; set; }

        /// <summary>
        /// Gets or sets the licensed namespace.
        /// </summary>
        public string? Namespace
        {
            get => (string?) this.GetFieldValue( LicenseFieldIndex.Namespace );
            set => this.SetFieldValue<LicenseFieldString>( LicenseFieldIndex.Namespace, value );
        }

        /// <summary>
        /// Gets or sets a value indicating whether the license usage can be audited.
        /// </summary>
        public bool? Auditable
        {
            get => (bool?) this.GetFieldValue( LicenseFieldIndex.Auditable );
            set => this.SetFieldValue<LicenseFieldBool>( LicenseFieldIndex.Auditable, value );
        }

        /// <summary>
        /// Gets or sets the licensed public key token.
        /// </summary>
        public byte[]? PublicKeyToken
        {
            get => (byte[]?) this.GetFieldValue( LicenseFieldIndex.PublicKeyToken );
            set => this.SetFieldValue<LicenseFieldBytes>( LicenseFieldIndex.PublicKeyToken, value );
        }

        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        public byte[]? Signature
        {
            get => (byte[]?) this.GetFieldValue( LicenseFieldIndex.Signature );
            set => this.SetFieldValue<LicenseFieldBytes>( LicenseFieldIndex.Signature, value );
        }

        /// <summary>
        /// Gets the identifier of the signature.
        /// </summary>
        public byte? SignatureKeyId
        {
            get => (byte?) this.GetFieldValue( LicenseFieldIndex.SignatureKeyId );
            private set => this.SetFieldValue<LicenseFieldByte>( LicenseFieldIndex.SignatureKeyId, value );
        }

        /// <summary>
        /// Gets or sets the allowed number of users.
        /// </summary>
        public short? UserNumber
        {
            get => (short?) this.GetFieldValue( LicenseFieldIndex.UserNumber );
            set => this.SetFieldValue<LicenseFieldInt16>( LicenseFieldIndex.UserNumber, value );
        }

        /// <summary>
        /// Gets or sets the date from which the license is valid.
        /// </summary>
        public DateTime? ValidFrom
        {
            get => (DateTime?) this.GetFieldValue( LicenseFieldIndex.ValidFrom );
            set => this.SetFieldValue<LicenseFieldDate>( LicenseFieldIndex.ValidFrom, value );
        }

        /// <summary>
        /// Gets or sets the date to which the license is valid.
        /// </summary>
        public DateTime? ValidTo
        {
            get => (DateTime?) this.GetFieldValue( LicenseFieldIndex.ValidTo );
            set => this.SetFieldValue<LicenseFieldDate>( LicenseFieldIndex.ValidTo, value );
        }

        public DateTime? SubscriptionEndDate
        {
            get => (DateTime?) this.GetFieldValue( LicenseFieldIndex.SubscriptionEndDate );
            set => this.SetFieldValue<LicenseFieldDate>( LicenseFieldIndex.SubscriptionEndDate, value );
        }

        /// <summary>
        /// Gets or sets the full name of the licensee.
        /// </summary>
        public string? Licensee
        {
            get => (string?) this.GetFieldValue( LicenseFieldIndex.Licensee );
            set => this.SetFieldValue<LicenseFieldString>( LicenseFieldIndex.Licensee, value );
        }

        /// <summary>
        /// Gets or sets the hash of the licensee name.
        /// </summary>
        public int? LicenseeHash
        {
            get => (int?) this.GetFieldValue( LicenseFieldIndex.LicenseeHash );
            set => this.SetFieldValue<LicenseFieldInt32>( LicenseFieldIndex.LicenseeHash, value );
        }

        /// <summary>
        /// Gets or sets the number of days in the grace period.
        /// </summary>
        public byte GraceDays
        {
            get => (byte?) this.GetFieldValue( LicenseFieldIndex.GraceDays ) ?? 30;
            set => this.SetFieldValue<LicenseFieldByte>( LicenseFieldIndex.GraceDays, value );
        }

        /// <summary>
        /// Gets or sets the number of percents of additional users allowed during the grace period.
        /// </summary>
        public byte? GracePercent
        {
            get => (byte?) this.GetFieldValue( LicenseFieldIndex.GracePercent );
            set => this.SetFieldValue<LicenseFieldByte>( LicenseFieldIndex.GracePercent, value );
        }

        /// <summary>
        /// Gets or sets the number of authorized devices per user.
        /// </summary>
        public byte? DevicesPerUser
        {
            get => (byte?) this.GetFieldValue( LicenseFieldIndex.DevicesPerUser );
            set => this.SetFieldValue<LicenseFieldByte>( LicenseFieldIndex.DevicesPerUser, value );
        }

        public bool? AllowInheritance
        {
            get => (bool?) this.GetFieldValue( LicenseFieldIndex.AllowInheritance );
            set => this.SetFieldValue<LicenseFieldBool>( LicenseFieldIndex.AllowInheritance, value );
        }

        public bool? LicenseServerEligible
        {
            get => (bool?) this.GetFieldValue( LicenseFieldIndex.LicenseServerEligible );
            set => this.SetFieldValue<LicenseFieldBool>( LicenseFieldIndex.LicenseServerEligible, value );
        }

        public Version? MinPostSharpVersion
        {
            get
            {
                var minPostSharpVersionString = (string?) this.GetFieldValue( LicenseFieldIndex.MinPostSharpVersion );
                return minPostSharpVersionString == null ? null : System.Version.Parse( minPostSharpVersionString );
            }
        }

        // Used for testing
        internal object? UnknownMustUnderstandField
        {
            get => null;
            set => this.SetUnknownFieldValue( true, value! );
        }

        // Used for testing
        internal object? UnknownOptionalField
        {
            get => null;
            set => this.SetUnknownFieldValue( false, value! );
        }
    }
}