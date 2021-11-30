// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses.LicenseFields;
using System;

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
                if (Product == LicensedProduct.PostSharp30 && _licenseType == LicenseType.Professional)
                {
                    return LicenseType.PerUser;
                }

                return _licenseType;
            }

            set => _licenseType = value;
        }
#pragma warning restore 618

        public string LicenseTypeString => LicenseType.ToString();

#pragma warning disable 618
        private LicensedProduct _product;

        /// <summary>
        /// Gets or sets the licensed product.
        /// </summary>
        public LicensedProduct Product
        {
            get
            {
                if (_product == LicensedProduct.PostSharp30)
                {
                    return _licenseType == LicenseType.Professional ? LicensedProduct.Framework : LicensedProduct.Ultimate;
                }

                return _product;
            }
            set => _product = value;
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
            get => (string?)GetFieldValue( LicenseFieldIndex.Namespace );
            set => SetFieldValue<LicenseFieldString>( LicenseFieldIndex.Namespace, value );
        }

        public bool Auditable
        {
            get
                => LicenseType switch
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    LicenseType.Site or LicenseType.Global or LicenseType.Anonymous => false,
#pragma warning restore CS0618                      // Type or member is obsolete
                    LicenseType.Evaluation => true, // We want to audit evaluation licenses so we know how people are using the product during evaluation.
                    _ => (bool?)GetFieldValue( LicenseFieldIndex.Auditable ) ?? true
                };

            set => SetFieldValue<LicenseFieldBool>( LicenseFieldIndex.Auditable, value );
        }

        /// <summary>
        /// Gets or sets the licensed public key token.
        /// </summary>
        public byte[]? PublicKeyToken
        {
            get => (byte[]?)GetFieldValue( LicenseFieldIndex.PublicKeyToken );
            set => SetFieldValue<LicenseFieldBytes>( LicenseFieldIndex.PublicKeyToken, value );
        }

        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        public byte[]? Signature
        {
            get => (byte[]?)GetFieldValue( LicenseFieldIndex.Signature );
            set => SetFieldValue<LicenseFieldBytes>( LicenseFieldIndex.Signature, value );
        }

        /// <summary>
        /// Gets the identifier of the signature.
        /// </summary>
        public byte? SignatureKeyId
        {
            get => (byte?)GetFieldValue( LicenseFieldIndex.SignatureKeyId );
            private set => SetFieldValue<LicenseFieldByte>( LicenseFieldIndex.SignatureKeyId, value );
        }

        /// <summary>
        /// Gets or sets the allowed number of users.
        /// </summary>
        public short? UserNumber
        {
            get => (short?)GetFieldValue( LicenseFieldIndex.UserNumber );
            set => SetFieldValue<LicenseFieldInt16>( LicenseFieldIndex.UserNumber, value );
        }

        /// <summary>
        /// Gets or sets the date from which the license is valid.
        /// </summary>
        public DateTime? ValidFrom
        {
            get => (DateTime?)GetFieldValue( LicenseFieldIndex.ValidFrom );
            set => SetFieldValue<LicenseFieldDate>( LicenseFieldIndex.ValidFrom, value );
        }

        /// <summary>
        /// Gets or sets the date to which the license is valid.
        /// </summary>
        public DateTime? ValidTo
        {
            get => (DateTime?)GetFieldValue( LicenseFieldIndex.ValidTo );
            set => SetFieldValue<LicenseFieldDate>( LicenseFieldIndex.ValidTo, value );
        }

        public DateTime? SubscriptionEndDate
        {
            get => (DateTime?)GetFieldValue( LicenseFieldIndex.SubscriptionEndDate );
            set => SetFieldValue<LicenseFieldDate>( LicenseFieldIndex.SubscriptionEndDate, value );
        }

        /// <summary>
        /// Gets or sets the full name of the licensee.
        /// </summary>
        public string? Licensee
        {
            get => (string?)GetFieldValue( LicenseFieldIndex.Licensee );
            set => SetFieldValue<LicenseFieldString>( LicenseFieldIndex.Licensee, value );
        }

        /// <summary>
        /// Gets or sets the hash of the licensee name.
        /// </summary>
        public int? LicenseeHash
        {
            get => (int?)GetFieldValue( LicenseFieldIndex.LicenseeHash );
            set => SetFieldValue<LicenseFieldInt32>( LicenseFieldIndex.LicenseeHash, value );
        }

        /// <summary>
        /// Gets or sets the number of days in the grace period.
        /// </summary>
        public byte GraceDays
        {
            get => (byte?)GetFieldValue( LicenseFieldIndex.GraceDays ) ?? 30;
            set => SetFieldValue<LicenseFieldByte>( LicenseFieldIndex.GraceDays, value );
        }

        /// <summary>
        /// Gets or sets the number of percents of additional users allowed during the grace period.
        /// </summary>
        public byte? GracePercent
        {
            get => (byte?)GetFieldValue( LicenseFieldIndex.GracePercent );
            set => SetFieldValue<LicenseFieldByte>( LicenseFieldIndex.GracePercent, value );
        }

        /// <summary>
        /// Gets or sets the number of authorized devices per user.
        /// </summary>
        public byte? DevicesPerUser
        {
            get => (byte?)GetFieldValue( LicenseFieldIndex.DevicesPerUser );
            set => SetFieldValue<LicenseFieldByte>( LicenseFieldIndex.DevicesPerUser, value );
        }

        public bool? AllowInheritance
        {
            get => (bool?)GetFieldValue( LicenseFieldIndex.AllowInheritance );
            set => SetFieldValue<LicenseFieldBool>( LicenseFieldIndex.AllowInheritance, value );
        }

        private bool? _isLicenseServerEligible;

        public bool LicenseServerEligible
        {
            get
            {
                if (_isLicenseServerEligible == null)
                {
                    const int lastLicenseIdBefore50Rtm = 100802;

                    var isExplicitlyLicenseServerEligible = (bool?)GetFieldValue( LicenseFieldIndex.LicenseServerEligible );

                    if (isExplicitlyLicenseServerEligible != null)
                    {
                        _isLicenseServerEligible = isExplicitlyLicenseServerEligible;
                    }
                    else if (LicenseType == LicenseType.PerUsage)
                    {
                        _isLicenseServerEligible = false;
                    }
                    else
                    {
                        _isLicenseServerEligible = LicenseId > 0 && LicenseId <= lastLicenseIdBefore50Rtm;
                    }
                }

                return _isLicenseServerEligible.Value;
            }

            set => SetFieldValue<LicenseFieldBool>( LicenseFieldIndex.LicenseServerEligible, _isLicenseServerEligible = value );
        }

        private Version? _minPostSharpVersion;

        // The getter of this property needs to be updated along with any
        // breaking change of the License class.
        public Version MinPostSharpVersion
        {
            get
            {
                if (_minPostSharpVersion == null)
                {
                    var minPostSharpVersionString = (string?)GetFieldValue( LicenseFieldIndex.MinPostSharpVersion );

                    if (minPostSharpVersionString != null)
                    {
                        _minPostSharpVersion = System.Version.Parse( minPostSharpVersionString );
                    }
                    else if (LicenseType == LicenseType.PerUsage || Product == LicensedProduct.CachingLibrary)
                    {
                        _minPostSharpVersion = new Version( 6, 6, 0 );
                    }
#pragma warning disable 618
                    else if (Product == LicensedProduct.PostSharp20)
                    {
                        _minPostSharpVersion = new Version( 2, 0, 0 );
                    }
                    else if (( Product == LicensedProduct.Ultimate || Product == LicensedProduct.Framework )
                             && LicenseType == LicenseType.Enterprise)
                    {
                        _minPostSharpVersion = new Version( 5, 0, 22 );
                    }
#pragma warning restore 618
                    else if (_isLicenseServerEligible != null)
                    {
                        _minPostSharpVersion = new Version( 5, 0, 22 );
                    }
                    else
                    {
                        _minPostSharpVersion = new Version( 3, 0, 0 );
                    }
                }

                return _minPostSharpVersion;
            }

            // Used for testing and backward-compatible license creation.
            internal set
            {
                if (_minPostSharpVersion != null)
                {
                    if (_minPostSharpVersion > MinPostSharpVersionValidationRemovedPostSharpVersion)
                    {
                        throw new ArgumentOutOfRangeException(
                            $"Since PostSharp {MinPostSharpVersionValidationRemovedPostSharpVersion}, " +
                            $"all license keys are backward compatible and the minimal PostSharp version is only considered by previous versions. " +
                            $"Set the {nameof(MinPostSharpVersion)} to \"{MinPostSharpVersionValidationRemovedPostSharpVersion}\" if the license is incompatible with versions prior to "
                            +
                            $"6.5.17, 6.8.10 and 6.9.3 or keep null if it is compatible." );
                    }
                }

                _minPostSharpVersion = value;
                SetFieldValue<LicenseFieldString>( LicenseFieldIndex.MinPostSharpVersion, value.ToString() );
            }
        }

        // Used for testing
        internal object? UnknownMustUnderstandField
        {
            get => null;
            set => SetUnknownFieldValue( true, value! );
        }

        // Used for testing
        internal object? UnknownOptionalField
        {
            get => null;
            set => SetUnknownFieldValue( false, value! );
        }
    }
}