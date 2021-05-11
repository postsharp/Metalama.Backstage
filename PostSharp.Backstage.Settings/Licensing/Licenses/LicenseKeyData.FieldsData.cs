// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Backstage.Licensing.Licenses.LicenseFields;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal partial class LicenseKeyData
    {
        public string? LicenseString { get; set; }

        /// <summary>
        /// Gets the license version.
        /// </summary>
        public byte Version { get; private set; }

        public Guid? LicenseGuid { get; set; }

#pragma warning disable 618
        private LicenseType _licenseType;

        /// <summary>
        /// Gets or sets the type of license.
        /// </summary>
        public LicenseType LicenseType
        {
            get
            {
                if ( this.Product == LicensedProduct.PostSharp30 && this._licenseType == Licenses.LicenseType.Professional )
                {
                    return Licenses.LicenseType.PerUser;
                }

                return this._licenseType;
            }

            set => this._licenseType = value;
        }
#pragma warning restore 618

        public string LicenseTypeString => this.LicenseType.ToString();

#pragma warning disable 618
        private LicensedProduct _product;

        /// <summary>
        /// Gets or sets the licensed product.
        /// </summary>
        public LicensedProduct Product
        {
            get
            {
                if ( this._product == LicensedProduct.PostSharp30 )
                {
                    return this._licenseType == Licenses.LicenseType.Professional ? LicensedProduct.Framework : LicensedProduct.Ultimate;
                }

                return this._product;
            }
            set => this._product = value;
        }
#pragma warning restore 618

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

        public bool Auditable
        {
            get => this.LicenseType switch
            {
#pragma warning disable CS0618 // Type or member is obsolete
                LicenseType.Site or LicenseType.Global or LicenseType.Anonymous => false,
#pragma warning restore CS0618 // Type or member is obsolete
                LicenseType.Evaluation => true, // We want to audit evaluation licenses so we know how people are using the product during evaluation.
                _ => (bool?) this.GetFieldValue( LicenseFieldIndex.Auditable ) ?? true,
            };

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

        private bool? _isLicenseServerEligible;

        public bool LicenseServerEligible
        {
            get
            {
                if ( this._isLicenseServerEligible == null )
                {
                    const int lastLicenseIdBefore50Rtm = 100802;

                    var isExplicitlyLicenseServerEligible = (bool?) this.GetFieldValue( LicenseFieldIndex.LicenseServerEligible );

                    if ( isExplicitlyLicenseServerEligible != null )
                    {
                        this._isLicenseServerEligible = isExplicitlyLicenseServerEligible;
                    }
                    else if ( this.LicenseType == LicenseType.PerUsage )
                    {
                        this._isLicenseServerEligible = false;
                    }
                    else
                    {
                        this._isLicenseServerEligible = this.LicenseId > 0 && this.LicenseId <= lastLicenseIdBefore50Rtm;
                    }
                }

                return this._isLicenseServerEligible.Value;
            }

            set => this.SetFieldValue<LicenseFieldBool>( LicenseFieldIndex.LicenseServerEligible, this._isLicenseServerEligible = value );
        }

        private Version? _minPostSharpVersion;

        // The getter of this property needs to be updated along with any
        // breaking change of the License class.
        public Version MinPostSharpVersion
        {
            get
            {
                if ( this._minPostSharpVersion == null )
                {
                    var minPostSharpVersionString = (string?) this.GetFieldValue( LicenseFieldIndex.MinPostSharpVersion );

                    if ( minPostSharpVersionString != null )
                    {
                        this._minPostSharpVersion = System.Version.Parse( minPostSharpVersionString );
                    }
                    else if ( this.LicenseType == LicenseType.PerUsage || this.Product == LicensedProduct.CachingLibrary )
                    {
                        this._minPostSharpVersion = new Version( 6, 6, 0 );
                    }
#pragma warning disable 618
                    else if ( this.Product == LicensedProduct.PostSharp20 )
                    {
                        this._minPostSharpVersion = new Version( 2, 0, 0 );
                    }
                    else if ( (this.Product == LicensedProduct.Ultimate || this.Product == LicensedProduct.Framework)
                              && this.LicenseType == LicenseType.Enterprise )
                    {
                        this._minPostSharpVersion = new Version( 5, 0, 22 );
                    }
#pragma warning restore 618
                    else if ( this._isLicenseServerEligible != null )
                    {
                        this._minPostSharpVersion = new Version( 5, 0, 22 );
                    }
                    else
                    {
                        this._minPostSharpVersion = new Version( 3, 0, 0 );
                    }
                }

                return this._minPostSharpVersion;
            }

            // Used for testing and backward-compatible license creation.
            internal set
            {
                if ( this._minPostSharpVersion != null )
                {
                    if ( this._minPostSharpVersion > MinPostSharpVersionValidationRemovedPostSharpVersion )
                    {
                        throw new ArgumentOutOfRangeException(
                            $"Since PostSharp {MinPostSharpVersionValidationRemovedPostSharpVersion}, " +
                            $"all license keys are backward compatible and the minimal PostSharp version is only considered by previous versions. " +
                            $"Set the {nameof( this.MinPostSharpVersion )} to \"{MinPostSharpVersionValidationRemovedPostSharpVersion}\" if the license is incompatible with versions prior to " +
                            $"6.5.17, 6.8.10 and 6.9.3 or keep null if it is compatible." );
                    }
                }

                this._minPostSharpVersion = value;
                this.SetFieldValue<LicenseFieldString>( LicenseFieldIndex.MinPostSharpVersion, value.ToString() );
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
