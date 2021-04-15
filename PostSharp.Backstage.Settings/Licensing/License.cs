// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PostSharp.Backstage.Licensing.Helpers;
using System.Linq;
using PostSharp.Backstage.Settings;
using PostSharp.Backstage.Utilities;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing
{

    /// <exclude />
    /// <summary>
    /// Encapsulates a PostSharp license.
    /// </summary>
    [Serializable]
    public class License
    {
        /// <summary>
        /// Since PostSharp 6.5.17, 6.8.10, and 6.9.3 the <see cref="MinPostSharpVersion" /> is no longer checked.
        /// For compatibility with previous version, all licenses with features introduced since these versions
        /// should have the <see cref="MinPostSharpVersion" />
        /// set to <see cref="MinPostSharpVersionValidationRemovedPostSharpVersion" />.
        /// </summary>
        public static readonly Version MinPostSharpVersionValidationRemovedPostSharpVersion = new Version( 6, 9, 3 );

        private readonly LicenseData data;

        [Serializable]
        private class LicenseData
        {
            private SortedDictionary<LicenseFieldIndex, LicenseField> fields = new SortedDictionary<LicenseFieldIndex, LicenseField>();

            public string LicenseString { get; set; }

            public byte Version { get; set; }

            public LicenseType LicenseType { get; set; }

            public int LicenseId { get; set; }

            public Guid? LicenseGuid { get; set; }

            /// <summary>
            /// Gets or sets the licensed product.
            /// </summary>
            public LicensedProduct Product { get; set; }


            public SortedDictionary<LicenseFieldIndex, LicenseField> Fields => this.fields;

            public LicenseData Clone()
            {
                LicenseData clone = (LicenseData) this.MemberwiseClone();
                clone.fields = new SortedDictionary<LicenseFieldIndex, LicenseField>();
                foreach ( KeyValuePair<LicenseFieldIndex, LicenseField> pair in this.fields )
                {
                    clone.fields.Add( pair.Key, pair.Value.Clone() );
                }

                return clone;
            }
        }


        /// <summary>
        /// Initializes a new empty <see cref="License"/>.
        /// </summary>
        public License()
        {
            this.data = new LicenseData();
            this.Version = 2;
        }

        internal License( object licenseData )
            : this( (LicenseData) licenseData )
        {
        }

        private License( LicenseData licenseData )
        {
            this.data = licenseData;
        }

        private static LicenseData ReadLicenseData( BinaryReader reader )
        {
            LicenseFieldIndex index;

            LicenseData data = new LicenseData
                               {
                                   Version = reader.ReadByte(),
                                   LicenseId = reader.ReadInt32(),
                                   LicenseType = (LicenseType) reader.ReadByte(),
                                   Product = (LicensedProduct) reader.ReadByte()
                               };

            while ( (index = (LicenseFieldIndex) reader.ReadByte()) != LicenseFieldIndex.End )
            {
                LicenseField licenseField;
                switch ( index )
                {
                    case LicenseFieldIndex.SignatureKeyId:
                    case LicenseFieldIndex.GraceDays:
                    case LicenseFieldIndex.GracePercent:
                    case LicenseFieldIndex.DevicesPerUser:
                        licenseField = new LicenseFieldByte();
                        break;

                    case LicenseFieldIndex.Auditable:
                    case LicenseFieldIndex.AllowInheritance:
                    case LicenseFieldIndex.LicenseServerEligible:
                        licenseField = new LicenseFieldBool();
                        break;

                    case LicenseFieldIndex.Licensee:
                    case LicenseFieldIndex.Namespace:
                    case LicenseFieldIndex.MinPostSharpVersion:
                        licenseField = new LicenseFieldString();
                        break;

                    case LicenseFieldIndex.UserNumber:
                        licenseField = new LicenseFieldInt16();
                        break;

                    case LicenseFieldIndex.PublicKeyToken:
                    case LicenseFieldIndex.Signature:
                        licenseField = new LicenseFieldBytes();
                        break;

                    case LicenseFieldIndex.ValidFrom:
                    case LicenseFieldIndex.ValidTo:
                    case LicenseFieldIndex.SubscriptionEndDate:
                        licenseField = new LicenseFieldDate();
                        break;

                    case LicenseFieldIndex.LicenseeHash:
                    case LicenseFieldIndex.Features:
                        licenseField = new LicenseFieldInt64();
                        break;

                    default:
                        if ( !IsFieldPrefixedByLength( index ) )
                        {
                            throw new InvalidLicenseException( "Unexpected license field." );
                        }

                        // This is an unknown field.
                        // We read its data to
                        // - Validate that we do understand the must-understand fields.
                        // - Keep the license integrity, e.g. for cloning or signature verification.
                        licenseField = new LicenseFieldBytes();
                        break;
                }

                licenseField.Read( reader );
                data.Fields.Add( index, licenseField );
            }

            // this works only for base streams that implement Length and Position but since we always use MemoryStream it is ok.
            if ( reader.BaseStream.Length > reader.BaseStream.Position )
            {
                throw new InvalidLicenseException( "License is too long." );
            }

            return data;
        }

        #region Mandatory fields

        /// <summary>
        /// Gets or sets the license version.
        /// </summary>
        public byte Version
        {
            get => this.data.Version;
            set => this.data.Version = value;
        }

#pragma warning disable 618
        /// <summary>
        /// Gets or sets the type of license.
        /// </summary>
        public LicenseType LicenseType
        {
            get
            {
                if ( this.data.Product == LicensedProduct.PostSharp30 && this.data.LicenseType == LicenseType.Professional )
                {
                    return LicenseType.PerUser;
                }
                return this.data.LicenseType;
            }
            set => this.data.LicenseType = value;
        }
#pragma warning restore 618

        public string LicenseTypeString => this.LicenseType.ToString();

#pragma warning disable 618
        /// <summary>
        /// Gets or sets the licensed product.
        /// </summary>
        public LicensedProduct Product
        {
            get
            {
                if ( this.data.Product == LicensedProduct.PostSharp30 )
                {
                    return this.data.LicenseType == LicenseType.Professional ? LicensedProduct.Framework : LicensedProduct.Ultimate;
                }
                return this.data.Product;
            }
            set => this.data.Product = value;
        }
#pragma warning restore 618

#pragma warning disable CA1721 // Property names should not match get methods (TODO)
        /// <summary>
        /// Gets or sets the identifier of the current license.
        /// </summary>
        public int LicenseId
#pragma warning restore CA1721 // Property names should not match get methods
        {
            get => this.data.LicenseId;
            set => this.data.LicenseId = value;
        }

        public string LicenseString => this.data.LicenseString;

        #endregion

        /// <summary>
        /// Gets a deep clone of the current <see cref="License"/>.
        /// </summary>
        /// <returns></returns>
        public License Clone()
        {
            License clone = new License( this.data.Clone() );

            return clone;
        }


        private byte[] GetSignedBuffer()
        {
            // Write the license to a buffer without the key.
            MemoryStream memoryStream = new MemoryStream();
            using ( BinaryWriter binaryWriter = new BinaryWriter( memoryStream ) )
            {
                this.Write( binaryWriter, false );
            }

            byte[] signedBuffer = memoryStream.ToArray();
            return signedBuffer;
        }

        /// <summary>
        /// Signs the current license.
        /// </summary>
        /// <param name="signatureKeyId">Identifier of the private key.</param>
        /// <param name="privateKey">XML representation of the private key.</param>
        public void Sign( byte signatureKeyId, string privateKey )
        {
            this.SignatureKeyId = signatureKeyId;
            byte[] signedBuffer = this.GetSignedBuffer();
            this.Signature = CryptoUtilities.Sign( signedBuffer, privateKey );
        }

        protected virtual bool RequiresSignature()
        {
            return true;
        }

        /// <summary>
        /// Verifies the signature of the current license.
        /// </summary>
        /// <param name="publicKey">Public key.</param>
        /// <returns><c>true</c> if the signature is correct, otherwise <c>false</c>.</returns>
        public bool VerifySignature( DSA publicKey )
        {
            if ( !this.RequiresSignature() )
                return true;

            if ( publicKey == null )
                throw new ArgumentNullException( nameof(publicKey) );

            if ( !this.RequiresSignature() )
                return true;

            byte[] signature = this.Signature;
            if ( signature == null )
                return false;

            return CryptoUtilities.VerifySignature( this.GetSignedBuffer(), publicKey, signature );
        }

        /// <summary>
        /// Gets the licensed packages provided by this license.
        /// </summary>
        /// <returns>The LicensedPackages provided by this license.</returns>
        public virtual LicensedPackages GetLicensedPackages()
        {
            return LicensedProductPackages.Community;
        }
        
        /// <summary>
        /// Calculates the rank of the license. The rank is the count of licensed packages that the license
        /// provides.
        /// </summary>
        /// <returns>The rank of the license.</returns>
        internal int LicensedPackagesRank()
        {
            int licensedPackages = (int) this.GetLicensedPackages();
            
            int result = 0;
            while (licensedPackages > 0)
            {
                result += licensedPackages & 1;
                licensedPackages >>= 1;
            }

            return result;
        }

        [Obsolete("Use GetLicensedPackages() instead")]
        public virtual IEnumerable<KeyValuePair<LicensedProduct, long>> GetLicensedFeatures()
        {
            yield return new KeyValuePair<LicensedProduct, long>( this.Product, this.Features.GetValueOrDefault() );
        }

        public virtual LicenseSource GetAllowedLicenseSources()
        {
            return LicenseSource.All;
        }

        /// <summary>
        /// Determines whether the license usage should be audited on a weekly basis.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAudited()
        {
            return false;
        }


        public virtual bool Validate( byte[]? publicKeyToken, IDateTimeProvider dateTimeProvider, out string errorDescription )
        {
            if ( !this.VerifySignature() )
            {
                errorDescription = "The license signature is invalid.";
                return false;
            }

            if ( this.ValidFrom.HasValue && this.ValidFrom > dateTimeProvider.GetCurrentDateTime() )
            {
                errorDescription = "The license is not yet valid.";
                return false;
            }

            if ( this.ValidTo.HasValue && this.ValidTo < dateTimeProvider.GetCurrentDateTime() )
            {
                errorDescription = "The license is not valid any more.";
                return false;
            }

            if ( this.PublicKeyToken != null )
            {
                if ( publicKeyToken == null )
                {
                    errorDescription = "The assembly is missing a public key token.";
                    return false;
                }

                if ( !ComparePublicKeyToken( publicKeyToken, this.PublicKeyToken ) )
                {
                    errorDescription = "The public key token of the assembly does not match the license.";
                    return false;
                }
            }

            if ( !Enum.IsDefined( typeof( LicenseType ), this.LicenseType ) )
            {
                errorDescription = "The license type is not known.";
                return false;
            }

            if ( !Enum.IsDefined( typeof( LicensedProduct ), this.Product ) )
            {
                errorDescription = "The licensed product is not known.";
                return false;
            }

            if ( this.data.Fields.Keys.Any( i =>
              IsMustUnderstandField( i )
              && !Enum.IsDefined( typeof( LicenseFieldIndex ), i ) ) )
            {
                errorDescription = "The license contains unknown must-understand fields.";
                return false;
            }

            errorDescription = null;
            return true;
        }

        private static bool ComparePublicKeyToken( byte[] key1, byte[] key2 )
        {
            if ( key1 == null )
                return key2 == null;

            if ( key2 == null )
                return false;

            for ( int i = 0; i < key1.Length; i++ )
                if ( key1[i] != key2[i] )
                    return false;

            return true;
        }

        /// <exclude/>
        public bool VerifySignature()
        {
            if ( !this.RequiresSignature() )
                return true;

            if ( !this.SignatureKeyId.HasValue )
                return false;

            DSA publicKey = CryptoUtilities.GetPublicKey( this.SignatureKeyId.GetValueOrDefault() );
            if ( publicKey == null )
                return false;

            try
            {
                return this.VerifySignature( publicKey );
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Serializes the current license into a string.
        /// </summary>
        /// <returns>A string representing the current license.</returns>
        public string Serialize()
        {
            /// <summary>
            /// Returns <c>true</c> if the licensed product has been present prior to PostSharp 6.5.17/6.8.10/6.9.3.
            /// </summary>
            /// <param name="licenseProduct"></param>
            /// <returns></returns>
            bool IsLicensedProductValidInAllPostSharpVersions( LicensedProduct licenseProduct )
            {
                switch ( licenseProduct )
                {
                    case LicensedProduct.None:
#pragma warning disable CS0618 // Type or member is obsolete
                    case LicensedProduct.PostSharp20:
                    case LicensedProduct.PostSharp30:
#pragma warning restore CS0618 // Type or member is obsolete
                    case LicensedProduct.Ultimate:
                    case LicensedProduct.Framework:
                    case LicensedProduct.DiagnosticsLibrary:
                    case LicensedProduct.ModelLibrary:
                    case LicensedProduct.ThreadingLibrary:
                    case LicensedProduct.CachingLibrary:
                        return true;
                    default:
                        return false;
                }
            }

            if ( !IsLicensedProductValidInAllPostSharpVersions( this.Product )
                || this.data.Fields.Keys.Any( i => IsFieldPrefixedByLength( i ) ) )
            {
                if ( this.MinPostSharpVersion != MinPostSharpVersionValidationRemovedPostSharpVersion )
                {
                    throw new InvalidOperationException( $"The license contains products or fields introduced " +
                        $"after PostSharp {MinPostSharpVersionValidationRemovedPostSharpVersion}. " +
                        $"Set the {nameof( this.MinPostSharpVersion )} property " +
                        $"to {nameof( License )}.{nameof( MinPostSharpVersionValidationRemovedPostSharpVersion )}." );
                }
            }

            MemoryStream memoryStream = new MemoryStream();
            using ( BinaryWriter binaryWriter = new BinaryWriter( memoryStream ) )
            {
                this.Write( binaryWriter, true );
                binaryWriter.Write( (byte) LicenseFieldIndex.End );
            }

            string prefix;
            if ( this.LicenseGuid.HasValue )
            {
                prefix = Base32.ToBase32String( this.LicenseGuid.Value.ToByteArray(), 0 );
            }
            else
            {
                prefix = this.LicenseId.ToString( CultureInfo.InvariantCulture );
            }

            this.data.LicenseString = prefix + "-" + Base32.ToBase32String( memoryStream.ToArray(), 0 );
            return this.data.LicenseString;
        }

        public static int GetLicenseId( string s )
        {
            int firstDash = s.IndexOf( '-' );

            string prefix = s.Substring( 0, firstDash );
            if ( firstDash > 0 && int.TryParse( prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out int licenseId ) )
            {
                return licenseId;
            }
            else
            {
                try
                {
                    byte[] guidBytes = Base32.FromBase32String( prefix );
                    if ( guidBytes != null && guidBytes.Length == 16 )
                        return 0;
                }
                catch
                {

                }
            }

            return -1;
        }

        /// <summary>
        /// Removes invalid characters from a license string.
        /// </summary>
        /// <param name="s">A license string.</param>
        /// <returns>The license string in canonical format.</returns>
        public static string CleanLicenseString( string s )
        {
            if ( s == null )
                return null;

            StringBuilder stringBuilder = new StringBuilder( s.Length );

            // Remove all spaces from the license.
            foreach ( char c in s )
            {
                if ( char.IsLetterOrDigit( c ) || c == '-' )
                {
                    stringBuilder.Append( c );
                }
            }

            return stringBuilder.ToString().ToUpperInvariant();
        }

        /// <summary>
        /// Deserializes a <see cref="License"/> from its string representation.
        /// </summary>
        /// <param name="licenseString">A serialized license.</param>
        /// <returns>The <see cref="License"/> constructed from <paramref name="licenseString"/>.</returns>
        public static bool TryDeserialize( string licenseString, IApplicationInfoService applicationInfoService, out License? license, ITrace? licensingTrace = null )
        {
            licenseString = CleanLicenseString( licenseString );

            if ( string.IsNullOrWhiteSpace( licenseString ) )
            {
                license = null;
                return false;
            }

            licensingTrace?.WriteLine( "Deserializing license {{{0}}}.", licenseString );
            Guid? licenseGuid = null;
            try
            {
                // Parse the license key prefix.
                int firstDash = licenseString.IndexOf( '-' );
                if ( firstDash < 0 )
                {
                    licensingTrace?.WriteLine( "License header not found for license {{{0}}}.", licenseString );
                    license = null;
                    return false;
                }

                string prefix = licenseString.Substring( 0, firstDash );
                if ( !int.TryParse( prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out int licenseId ) )
                {
                    // If this is not an integer, this may be a GUID.
                    licenseGuid = new Guid( Base32.FromBase32String( prefix ) );
                }

                byte[] licenseBytes = Base32.FromBase32String( licenseString.Substring( firstDash + 1 ) );
                MemoryStream memoryStream = new MemoryStream( licenseBytes );
                using ( BinaryReader reader = new BinaryReader( memoryStream ) )
                {
                    LicenseData licenseData = ReadLicenseData( reader );

                    if ( licenseData.LicenseId != licenseId )
                    {
                        throw new InvalidLicenseException(
                            string.Format(CultureInfo.InvariantCulture, "The license id in the body ({0}) does not match the header for license {{{1}}}.",
                                           licenseId,
                                           licenseString ) );
                    }

                    licenseData.LicenseGuid = licenseGuid;
                    licenseData.LicenseString = licenseString;

                    switch ( licenseData.Product )
                    {
#pragma warning disable 618
                        case LicensedProduct.Ultimate:
                        case LicensedProduct.Framework:
                        case LicensedProduct.PostSharp30:
                            license = new CoreLicense( licenseData, applicationInfoService.Version, applicationInfoService.BuildDate );
                            break;
#pragma warning restore 618

                        case LicensedProduct.DiagnosticsLibrary:
                        case LicensedProduct.ModelLibrary:
                        case LicensedProduct.ThreadingLibrary:
                        case LicensedProduct.CachingLibrary:
                            license = new LibraryLicense( licenseData, applicationInfoService.Version, applicationInfoService.BuildDate );
                            break;

                        default:
                            license = new License( licenseData );
                            break;
                    }

                    licensingTrace?.WriteLine( "Deserialized license: {0}", license.ToString() );
                    return true;
                }
            }
            catch ( Exception e )
            {
                licensingTrace?.WriteLine( "Cannot parse the license {{{0}}}: {1}", licenseString, e );
                license = null;
                return false;
            }
        }

        private void Write( BinaryWriter writer, bool includeAll )
        {
            writer.Write( this.Version );
            writer.Write( this.LicenseId );
            writer.Write( (byte) this.data.LicenseType );
            writer.Write( (byte) this.data.Product );


            foreach ( KeyValuePair<LicenseFieldIndex, LicenseField> pair in this.data.Fields )
            {
                switch ( pair.Key )
                {
                    case LicenseFieldIndex.Signature:
                        if ( includeAll )
                        {
                            goto default;
                        }
                        continue;

                    default:
                        writer.Write( (byte) pair.Key );

                        if ( IsFieldPrefixedByLength( pair.Key ) )
                        {
                            pair.Value.WriteConstantLength( writer );
                        }

                        pair.Value.Write( writer );
                        break;
                }
            }
        }


        private object GetFieldValue( LicenseFieldIndex index )
        {
            if ( this.data.Fields.TryGetValue( index, out LicenseField licenseField ) )
                return licenseField.Value;
            else
                return null;
        }

        // Used for testing
        private void SetUnknownFieldValue( bool mustUnderstand, object value )
        {
            LicenseFieldIndex index = (LicenseFieldIndex) (mustUnderstand ? 128 : 253);

            switch ( value.GetType().Name )
            {
                case nameof( Boolean ):
                    this.SetFieldValue<LicenseFieldBool>( index, value );
                    break;

                case nameof( Byte ):
                    this.SetFieldValue<LicenseFieldByte>( index, value );
                    break;

                case nameof( Int16 ):
                    this.SetFieldValue<LicenseFieldInt16>( index, value );
                    break;

                case nameof( Int32 ):
                    this.SetFieldValue<LicenseFieldInt32>( index, value );
                    break;

                case nameof( Int64 ):
                    this.SetFieldValue<LicenseFieldInt64>( index, value );
                    break;

                case nameof( DateTime ):
                    this.SetFieldValue<LicenseFieldDate>( index, ((DateTime) value).Date );
                    this.SetFieldValue<LicenseFieldDateTime>( index, value );
                    break;

                case nameof( String ):
                    this.SetFieldValue<LicenseFieldString>( index, value );
                    break;

                case nameof( Byte ) + "[]":
                    this.SetFieldValue<LicenseFieldBytes>( index, value );
                    break;

                case nameof( Guid ):
                    this.SetFieldValue<LicenseFieldGuid>( index, value );
                    break;

                default:
                    throw new ArgumentException( $"License fields of type {value.GetType()} are not supported." );
            }
        }

        private void SetFieldValue<T>( LicenseFieldIndex index, object value )
            where T : LicenseField, new()
        {
            if ( value == null )
            {
                this.data.Fields.Remove( index );
            }
            else
            {
                this.data.Fields[index] = new T {Value = value};
            }
        }

        /// <summary>
        /// Gets or sets the set of features explicitly set in this license.
        /// </summary>
        [Obsolete("Use GetLicensedPackages instead.")]
        public long? Features
        {
            get => (long?) this.GetFieldValue( LicenseFieldIndex.Features );
            set => this.SetFieldValue<LicenseFieldInt64>( LicenseFieldIndex.Features, value );
        }

        public Guid? LicenseGuid
        {
            get => this.data.LicenseGuid;
            set => this.data.LicenseGuid = value;
        }

        public string LicenseUniqueId =>
            this.LicenseGuid.HasValue ? this.LicenseGuid.Value.ToString() : this.LicenseId.ToString( CultureInfo.InvariantCulture );

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
        public string Licensee
        {
            get => (string) this.GetFieldValue( LicenseFieldIndex.Licensee );
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
        /// Gets or sets the licensed namespace.
        /// </summary>
        public string Namespace
        {
            get => (string) this.GetFieldValue( LicenseFieldIndex.Namespace );
            set => this.SetFieldValue<LicenseFieldString>( LicenseFieldIndex.Namespace, value );
        }

        /// <summary>
        /// Returns <c>true</c> when the <see cref="Namespace"/> property is set, otherwise <c>false</c>.
        /// </summary>
        public bool IsLimitedByNamespace => !string.IsNullOrEmpty( this.Namespace );

        public bool? Auditable
        {
            get => (bool?) this.GetFieldValue( LicenseFieldIndex.Auditable );
            set => this.SetFieldValue<LicenseFieldBool>( LicenseFieldIndex.Auditable, value );
        }

        /// <summary>
        /// Gets or sets the licensed public key token.
        /// </summary>
        #pragma warning disable CA1819 // Properties should not return arrays (TODO)
        public byte[] PublicKeyToken
        {
            get { return (byte[]) this.GetFieldValue( LicenseFieldIndex.PublicKeyToken ); }
            set { this.SetFieldValue<LicenseFieldBytes>( LicenseFieldIndex.PublicKeyToken, value ); }
        }
        
        /// <summary>
        /// Gets or sets the signature.
        /// </summary>
        public byte[] Signature
        {
            get => (byte[]) this.GetFieldValue( LicenseFieldIndex.Signature );
            set => this.SetFieldValue<LicenseFieldBytes>( LicenseFieldIndex.Signature, value );
        }
        #pragma warning restore CA1819 // Properties should not return arrays (TODO)


        /// <summary>
        /// Gets or sets the identifier of the signature.
        /// </summary>
        public byte? SignatureKeyId
        {
            get => (byte?) this.GetFieldValue( LicenseFieldIndex.SignatureKeyId );
            private set => this.SetFieldValue<LicenseFieldByte>( LicenseFieldIndex.SignatureKeyId, value );
        }

        /// <summary>
        /// Gets or sets the number of days in the grace period.
        /// </summary>
        public byte? GraceDays
        {
            get => (byte?) this.GetFieldValue( LicenseFieldIndex.GraceDays );
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

        private Version minPostSharpVersion;

        // The getter of this property needs to be updated along with any
        // breaking change of the License class.
        public Version MinPostSharpVersion
        {
            get
            {
                if ( this.minPostSharpVersion == null )
                {
                    string minPostSharpVersionString = (string) this.GetFieldValue( LicenseFieldIndex.MinPostSharpVersion );

                    if ( minPostSharpVersionString != null )
                    {
                        this.minPostSharpVersion = System.Version.Parse( minPostSharpVersionString );
                    }
                    else if ( this.LicenseType == LicenseType.PerUsage || this.Product == LicensedProduct.CachingLibrary )
                    {
                        this.minPostSharpVersion = new Version( 6, 6, 0 );
                    }
#pragma warning disable 618
                    else if ( this.Product == LicensedProduct.PostSharp20 )
                    {
                        this.minPostSharpVersion = new Version( 2, 0, 0 );
                    }
                    else if ( (this.Product == LicensedProduct.Ultimate || this.Product == LicensedProduct.Framework)
                              && this.LicenseType == LicenseType.Enterprise )
                    {
                        this.minPostSharpVersion = new Version( 5, 0, 22 );
                    }
#pragma warning restore 618
                    else if ( this.LicenseServerEligible != null )
                    {
                        this.minPostSharpVersion = new Version( 5, 0, 22 );
                    }
                    else
                    {
                        this.minPostSharpVersion = new Version( 3, 0, 0 );
                    }
                }

                return this.minPostSharpVersion;
            }

            // Used for testing and backward-compatible license creation.
            internal set
            {
                if ( this.minPostSharpVersion != null )
                {
                    if ( this.minPostSharpVersion > MinPostSharpVersionValidationRemovedPostSharpVersion )
                    {
                        throw new ArgumentOutOfRangeException(
                            $"Since PostSharp {MinPostSharpVersionValidationRemovedPostSharpVersion}, " +
                            $"all license keys are backward compatible and the minimal PostSharp version is only considered by previous versions. " +
                            $"Set the {nameof( this.MinPostSharpVersion )} to \"{MinPostSharpVersionValidationRemovedPostSharpVersion}\" if the license is incompatible with versions prior to " +
                            $"6.5.17, 6.8.10 and 6.9.3 or keep null if it is compatible." );
                    }
                }

                this.minPostSharpVersion = value;
                this.SetFieldValue<LicenseFieldString>( LicenseFieldIndex.MinPostSharpVersion, value.ToString() );
            }
        }

        // Used for testing
        internal object UnknownMustUnderstandField
        {
            get => null;
            
            set
            {
                this.SetUnknownFieldValue( true, value );
            }
        }

        // Used for testing
        internal object UnknownOptionalField
        {
            get => null;

            set
            {
                this.SetUnknownFieldValue( false, value );
            }
        }

        private static readonly Version version50 = new Version(5, 0, 0);

        internal bool RequiresVersionSpecificStore => this.MinPostSharpVersion >= version50;

        public virtual string GetProductName( bool detailed = false )
        {
            return this.Product.ToString();
        }

        public string GetLicenseTypeName()
        {
#pragma warning disable 618
            switch ( this.LicenseType )
            {
                case LicenseType.Community:
                    // This case is handled in CoreLicense.GetProductName().
                    return null;
                case LicenseType.Enterprise:
                case LicenseType.PerUser:
                    return "Per-Developer Subscription";
                case LicenseType.Site:
                    return "Site License";
                case LicenseType.Global:
                    return "Global License";
                case LicenseType.Evaluation:
                    return "Evaluation License";
                case LicenseType.Academic:
                    return "Academic License";
                case LicenseType.CommercialRedistribution:
                    return "Commercial Redistribution License";
                case LicenseType.PerUsage:
                    return "Per-Usage Subscription";
                default:
                    return null;
            }
#pragma warning restore 618
        }

        public virtual bool IsEvaluationLicense()
        {
            return false;
        }

        public virtual bool IsUserLicense()
        {
            return true;
        }

        /// <summary>
        /// Gets the number of days of the grace period, and returns the default value if the <see cref="GraceDays"/>
        /// property is not defined.
        /// </summary>
        /// <returns>The number of days of the grace period</returns>
        public virtual int GetGraceDaysOrDefault()
        {
            if ( this.GraceDays.HasValue )
                return this.GraceDays.Value;
            else
                return 0;
        }

        public virtual bool IsLicenseServerEligible()
        {
            const int lastLicenseIdBefore50Rtm = 100802;

            if ( this.LicenseServerEligible.HasValue )
            {
                return this.LicenseServerEligible.Value;
            }

            if ( this.LicenseType == LicenseType.PerUsage )
            {
                return false;
            }

            return this.LicenseId > 0 && this.LicenseId <= lastLicenseIdBefore50Rtm;
        }

        public virtual bool RequiresRevocationCheck()
        {
            return false;
        }

        public virtual bool RequiresWatermark()
        {
            return false;
        }


        /// <summary>
        /// Gets the number of authorized devices per user, and returns the default value if the <see cref="DevicesPerUser"/> 
        /// property is not defined.
        /// </summary>
        /// <returns></returns>
        public int GetDevicesPerUserOrDefault()
        {
            return this.DevicesPerUser.GetValueOrDefault( 2 );
        }

        /// <summary>
        /// Gets the number of percents of additional users allowed during the grace period, and returns the default
        /// value if the <see cref="GracePercent"/> property is not defined.
        /// </summary>
        public int GetGracePercentOrDefault()
        {
            return this.GracePercent.GetValueOrDefault( 30 );
        }

        public bool AllowsNamespace( string ns )
        {
            if ( !this.IsLimitedByNamespace )
            {
                return true;
            }

            if ( !ns.StartsWith( this.Namespace, StringComparison.OrdinalIgnoreCase ) )
            {
                return false;
            }

            if ( ns.Length == this.Namespace.Length )
            {
                return true;
            }

            char delimiter = ns[this.Namespace.Length];

            if (

                // If there is not '.' after the namespace name,
                // it means the namespace name is different
                // and it only begins with the required name.
                delimiter == '.' 

                // When we get a assembly full assembly name,
                // there is a ',' after the short name.
                || delimiter == ',' )
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "Version={0}, LicenseId={1}, LicenseType={2}, Product={3}", this.Version, this.LicenseId, this.LicenseType,
                                        this.Product );
            foreach ( KeyValuePair<LicenseFieldIndex, LicenseField> licenseField in this.data.Fields )
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ", {0}={1}", licenseField.Key, licenseField.Value );
            }

            return stringBuilder.ToString();
        }

        public virtual bool Upgrade()
        {
            return false;
        }

        /// <summary>
        /// Identifies a field. It is given as the first byte of a field binary data.
        /// </summary>
        private enum LicenseFieldIndex : byte
        {
            Features = 1,
            ValidFrom = 2,
            ValidTo = 3,
            Licensee = 4,
            Namespace = 5,
            PublicKeyToken = 8,
            UserNumber = 9,
            Signature = 10,
            SignatureKeyId = 11,
            LicenseeHash = 12,
            GraceDays = 15,
            GracePercent = 16,
            DevicesPerUser = 17,
            SubscriptionEndDate = 18,
            Auditable = 19,
            AllowInheritance = 20,
            LicenseServerEligible = 21,
            // 128 is reserved as unknown must-understand field for testing purposes
            // 253 is reserved as unknown optional field for testing purposes
            MinPostSharpVersion = 254,
            End = 255,
        }

        /// <summary>
        /// Returns <c>true</c> if a license field data contain its length.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <remarks>
        /// Till PostSharp 6.5.16/6.8.9/6.9.2, all license fields had to be known.
        /// Adding a field used to cause backward incompatibility.
        /// In the next versions and in Caravela, we require each new filed index
        /// to be followed by its length (1 byte).
        /// If such field is unknown, the given number of bytes is ignored.
        /// </remarks>
        private static bool IsFieldPrefixedByLength( LicenseFieldIndex index )
        {
            byte i = (byte) index;

            if ( i < 1 || i > 255 )
            {
                throw new ArgumentOutOfRangeException( nameof( index ) );
            }

            return i > 21 && i < 254;
        }

        /// <summary>
        /// Returns <c>true</c> if a license field must be understood.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <remarks>
        /// Till PostSharp 6.5.16/6.8.9/6.9.2, all license fields had to be understood.
        /// Adding a field used to cause backward incompatibility.
        /// In the next versions and in Caravela, we allow fields of indexes 129-253
        /// to be followed by its length (1 byte).
        /// If such field is unknown, the given number of bytes is ignored.
        /// </remarks>
        private static bool IsMustUnderstandField( LicenseFieldIndex index )
        {
            byte i = (byte) index;

            if ( i < 1 || i > 255 )
            {
                throw new ArgumentOutOfRangeException( nameof( index ) );
            }

            return i <= 128 || i >= 254;
        }

        [Serializable]
        private abstract class LicenseField
        {
            public object Value { get; set; }

            public abstract void Write( BinaryWriter writer );

            protected abstract bool TryGetConstantLength( out byte length );

            /// <summary>
            /// Writes the length of the field if it has a constant length.
            /// </summary>
            /// <remarks>
            /// For fields with dynamic length, the length is written as part of the payload
            /// to the same position.
            /// </remarks>
            /// <param name="writer"></param>
            public void WriteConstantLength( BinaryWriter writer )
            {
                if ( !this.TryGetConstantLength( out byte length ) )
                {
                    return;
                }

                writer.Write( length );
            }

            public abstract void Read( BinaryReader reader );

            public override string ToString()
            {
                return this.Value == null ? "null" : this.Value.ToString();
            }

            public LicenseField Clone()
            {
                return (LicenseField) this.MemberwiseClone();
            }
        }

        [Serializable]
        private sealed class LicenseFieldByte : LicenseField
        {
            public override void Write( BinaryWriter writer )
            {
                writer.Write( (byte) this.Value );
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                length = sizeof( byte );
                return true;
            }

            public override void Read( BinaryReader reader )
            {
                this.Value = reader.ReadByte();
            }
        }

        [Serializable]
        private sealed class LicenseFieldBool : LicenseField
        {
            private bool isBuggy;

            public override void Write( BinaryWriter writer )
            {
                if ( this.isBuggy )
                {
                    writer.Write( (bool) this.Value ? 1 : 0 );
                }
                else
                {
                    writer.Write( (byte) ((bool) this.Value ? 1 : 0) );
                }
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                if ( this.isBuggy )
                {
                    throw new InvalidOperationException( "Boolean license fields requiring the length to be serialized should no longer be buggy." );
                }

                length = sizeof( byte );
                return true;
            }

            public override void Read( BinaryReader reader )
            {
                this.Value = reader.ReadByte() != 0;

                // Check if the field was emitted as a int as a result of a bug.
                this.isBuggy = false;
                if ( reader.BaseStream.Length > reader.BaseStream.Position )
                {
                    if ( reader.ReadByte() == 0 )
                    {
                        reader.ReadByte();
                        reader.ReadByte();
                        this.isBuggy = true;
                    }
                    else
                    {
                        reader.BaseStream.Seek( -1, SeekOrigin.Current );
                    }
                }
            }
        }

        [Serializable]
        private sealed class LicenseFieldInt16 : LicenseField
        {
            public override void Write( BinaryWriter writer )
            {
                writer.Write( (short) this.Value );
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                length = sizeof( short );
                return true;
            }

            public override void Read( BinaryReader reader )
            {
                this.Value = reader.ReadInt16();
            }
        }

        [Serializable]
        private sealed class LicenseFieldInt32 : LicenseField
        {
            public override void Write( BinaryWriter writer )
            {
                writer.Write( (int) this.Value );
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                length = sizeof( int );
                return true;
            }

            public override void Read( BinaryReader reader )
            {
                this.Value = reader.ReadInt32();
            }
        }

        [Serializable]
        private sealed class LicenseFieldInt64 : LicenseField
        {
            public override void Write( BinaryWriter writer )
            {
                writer.Write( (long) this.Value );
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                length = sizeof( long );
                return true;
            }

            public override void Read( BinaryReader reader )
            {
                this.Value = reader.ReadInt64();
            }
        }

        [Serializable]
        private class LicenseFieldDate : LicenseField
        {
            private static readonly DateTime referenceDate = new DateTime( 2010, 1, 1 );


            public override void Write( BinaryWriter writer )
            {
                ushort days = (ushort) ((DateTime) this.Value).Subtract( referenceDate ).TotalDays;
                writer.Write( days );
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                length = sizeof( ushort );
                return true;
            }

            public override void Read( BinaryReader reader )
            {
                ushort days = reader.ReadUInt16();
                this.Value = referenceDate.AddDays( days );
            }
        }

        [Serializable]
        private class LicenseFieldDateTime : LicenseField
        {
            public override void Write( BinaryWriter writer )
            {
                long data = ((DateTime) this.Value).ToUniversalTime().ToBinary();
                writer.Write( data );
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                length = sizeof( long );
                return true;
            }

            public override void Read( BinaryReader reader )
            {
                long data = reader.ReadInt64();
                this.Value = DateTime.FromBinary( data ).ToLocalTime();
            }
        }

        [Serializable]
        private sealed class LicenseFieldString : LicenseField
        {
            public override void Write( BinaryWriter writer )
            {
                byte[] bytes = Encoding.UTF8.GetBytes( ((string) this.Value).Normalize() );
                if ( bytes.Length > 255 )
                    throw new InvalidOperationException( "Cannot have strings of more than 255 characters." );
                writer.Write( (byte) bytes.Length );
                writer.Write( bytes );
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                // The length of this field is variable.
                length = 0;
                return false;
            }

            public override void Read( BinaryReader reader )
            {
                byte[] bytes = reader.ReadBytes( reader.ReadByte() );
                this.Value = Encoding.UTF8.GetString( bytes );
            }
        }

        [Serializable]
        private sealed class LicenseFieldBytes : LicenseField
        {
            public override void Write( BinaryWriter writer )
            {
                byte[] bytes = (byte[]) this.Value;
                if ( bytes.Length > 255 )
                    throw new InvalidOperationException( "Cannot have buffers of more than 255 bytes." );

                writer.Write( (byte) bytes.Length );
                writer.Write( bytes );
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                // The length of this field is variable.
                length = 0;
                return false;
            }

            public override void Read( BinaryReader reader )
            {
                this.Value = reader.ReadBytes( reader.ReadByte() );
            }

            public override string ToString()
            {
                if ( this.Value == null )
                    return "null";
                return HexHelper.FormatBytes( (byte[]) this.Value );
            }
        }

        [Serializable]
        private sealed class LicenseFieldGuid : LicenseField
        {
            private const byte sizeOfGuidByteArray = 16;

            public override void Write( BinaryWriter writer )
            {
                Guid guid = (Guid) this.Value;
                writer.Write( guid.ToByteArray() );
            }

            protected override bool TryGetConstantLength( out byte length )
            {
                length = sizeOfGuidByteArray;
                return true;
            }

            public override void Read( BinaryReader reader )
            {
                this.Value = new Guid( reader.ReadBytes( sizeOfGuidByteArray ) );
            }

            public override string ToString()
            {
                if ( this.Value == null )
                    return "null";

                return ((Guid) this.Value).ToString();
            }
        }

        public ReportedLicense GetReportedLicense()
        {
            return new ReportedLicense( this.Product.ToString(), this.LicenseType.ToString() );
        }
    }

    [Serializable]
    public sealed class LicenseConfiguration
    {
        internal LicenseConfiguration( string licenseString, License? license, LicenseSource source, string sourceDescription )
        {
            this.LicenseString = licenseString ?? throw new ArgumentNullException( nameof(licenseString) );
            this.License = license;
            this.Source = source;
            this.SourceDescription = sourceDescription;
            this.IsLicenseServerUrl = LicenseServerClient.IsLicenseServerUrl( this.LicenseString );
        }

        public bool IsLicenseServerUrl { get; private set; }

        public string LicenseString { get; private set; }

        public LicenseLease? LicenseLease { get; internal set; }

        public License? License { get; internal set; }

        public LicenseSource Source { get; private set; }

        public string SourceDescription { get; set; }

        public override string ToString()
        {
            if ( this.License != null && this.License.LicenseType == LicenseType.Unattended )
            {
                return "Unattended Build";
            }
            else
            {
                return string.Format( CultureInfo.InvariantCulture, "{0} from {1} ({2})",
                                      this.License != null ? this.License.LicenseString : this.LicenseString,
                                      this.SourceDescription, this.Source );
            }
        }

        public ReportedLicense GetReportedLicense()
        {
            if ( this.License == null )
            {
                return new ReportedLicense("LicenseServerUnknown", "None");
            }
            else
            {
                return this.License.GetReportedLicense();
            }
        }
    }
}