// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using PostSharp.Backstage.Utilities;
using PostSharp.Backstage.Extensibility;
using System.Diagnostics.CodeAnalysis;

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
        /// Initializes a new empty <see cref="License"/>.
        /// </summary>
        public License()
        {
            this._privateData = new PrivateLicenseData();
            this.Version = 2;
        }

        internal License( object licenseData )
            : this( (PrivateLicenseData) licenseData )
        {
        }

        private License( PrivateLicenseData licenseData )
        {
            this._privateData = licenseData;
        }

        private static PrivateLicenseData ReadLicenseData( BinaryReader reader )
        {
            LicenseFieldIndex index;

            PrivateLicenseData data = new PrivateLicenseData
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
#pragma warning disable CS0618 // Type or member is obsolete
                    case LicenseFieldIndex.Features:
#pragma warning restore CS0618 // Type or member is obsolete
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

        /// <summary>
        /// Gets a deep clone of the current <see cref="License"/>.
        /// </summary>
        /// <returns></returns>
        public License Clone()
        {
            License clone = new License( this._privateData.Clone() );

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

            if ( this.ValidFrom.HasValue && this.ValidFrom > dateTimeProvider.Now )
            {
                errorDescription = "The license is not yet valid.";
                return false;
            }

            if ( this.ValidTo.HasValue && this.ValidTo < dateTimeProvider.Now )
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

            if ( this._privateData.Fields.Keys.Any( i =>
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
                || this._privateData.Fields.Keys.Any( i => IsFieldPrefixedByLength( i ) ) )
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

            this._privateData.LicenseString = prefix + "-" + Base32.ToBase32String( memoryStream.ToArray(), 0 );
            return this._privateData.LicenseString;
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
        public static bool TryDeserialize( string licenseString, IApplicationInfoService applicationInfoService, [MaybeNullWhen( returnValue: false )] out License license, ITrace? licensingTrace = null )
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
                    PrivateLicenseData licenseData = ReadLicenseData( reader );

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
            writer.Write( (byte) this._privateData.LicenseType );
            writer.Write( (byte) this._privateData.Product );


            foreach ( KeyValuePair<LicenseFieldIndex, LicenseField> pair in this._privateData.Fields )
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


        

        public Guid? LicenseGuid
        {
            get => this._privateData.LicenseGuid;
            set => this._privateData.LicenseGuid = value;
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

        public virtual string GetProductName( bool detailed = false )
        {
            return this.Product.ToString();
        }

        public virtual bool IsEvaluationLicense()
        {
            return false;
        }

        public virtual bool IsUserLicense()
        {
            return true;
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
            foreach ( KeyValuePair<LicenseFieldIndex, LicenseField> licenseField in this._privateData.Fields )
            {
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ", {0}={1}", licenseField.Key, licenseField.Value );
            }

            return stringBuilder.ToString();
        }

        public virtual bool Upgrade()
        {
            return false;
        }

        public ReportedLicense GetReportedLicense()
        {
            return new ReportedLicense( this.Product.ToString(), this.LicenseType.ToString() );
        }
    }
}