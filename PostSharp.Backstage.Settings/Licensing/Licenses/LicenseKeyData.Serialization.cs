// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using PostSharp.Backstage.Licensing.Cryptography;
using PostSharp.Backstage.Licensing.Licenses.LicenseFields;
using PostSharp.Backstage.Utilities;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal partial class LicenseKeyData
    {
        public static LicenseKeyData Deserialize( BinaryReader reader )
        {
            LicenseFieldIndex index;

            var data = new LicenseKeyData
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
                        if ( !index.IsPrefixedByLength() )
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
                data._fields.Add( index, licenseField );
            }

            // this works only for base streams that implement Length and Position but since we always use MemoryStream it is ok.
            if ( reader.BaseStream.Length > reader.BaseStream.Position )
            {
                throw new InvalidLicenseException( "License is too long." );
            }

            return data;
        }

        /// <summary>
        /// Serializes the current license key data into a string.
        /// </summary>
        /// <returns>A string representing the current license key data.</returns>
        public string Serialize()
        {
            // Returns <c>true</c> if the licensed product has been present prior to PostSharp 6.5.17/6.8.10/6.9.3.
            static bool IsLicensedProductValidInAllPostSharpVersions( LicensedProduct licenseProduct )
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
                || this._fields.Keys.Any( i => i.IsPrefixedByLength() ) )
            {
                if ( this.MinPostSharpVersion != MinPostSharpVersionValidationRemovedPostSharpVersion )
                {
                    throw new InvalidOperationException( $"The license contains products or fields introduced " +
                        $"after PostSharp {MinPostSharpVersionValidationRemovedPostSharpVersion}. " +
                        $"Set the {nameof( this.MinPostSharpVersion )} property " +
                        $"to {this.GetType().Name}.{nameof( MinPostSharpVersionValidationRemovedPostSharpVersion )}." );
                }
            }

            var memoryStream = new MemoryStream();
            using ( var binaryWriter = new BinaryWriter( memoryStream ) )
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

            this.LicenseString = prefix + "-" + Base32.ToBase32String( memoryStream.ToArray(), 0 );
            return this.LicenseString;
        }

        private void Write( BinaryWriter writer, bool includeAll )
        {
            writer.Write( this.Version );
            writer.Write( this.LicenseId );
            writer.Write( (byte) this._licenseType );
            writer.Write( (byte) this._product );

            foreach ( var pair in this._fields )
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

                        if ( pair.Key.IsPrefixedByLength() )
                        {
                            pair.Value.WriteConstantLength( writer );
                        }

                        pair.Value.Write( writer );
                        break;
                }
            }
        }

        private byte[] GetSignedBuffer()
        {
            // Write the license to a buffer without the key.
            var memoryStream = new MemoryStream();
            using ( var binaryWriter = new BinaryWriter( memoryStream ) )
            {
                this.Write( binaryWriter, false );
            }

            var signedBuffer = memoryStream.ToArray();
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
            var signedBuffer = this.GetSignedBuffer();
            this.Signature = LicenseCryptography.Sign( signedBuffer, privateKey );
        }

        /// <summary>
        /// Verifies the signature of the current license.
        /// </summary>
        /// <param name="publicKey">Public key.</param>
        /// <returns><c>true</c> if the signature is correct, otherwise <c>false</c>.</returns>
        public bool VerifySignature( DSA publicKey )
        {
            if ( !this.RequiresSignature )
            {
                return true;
            }

            if ( publicKey == null )
            {
                throw new ArgumentNullException( nameof( publicKey ) );
            }

            var signature = this.Signature;
            if ( signature == null )
            {
                return false;
            }

            return LicenseCryptography.VerifySignature( this.GetSignedBuffer(), publicKey, signature );
        }
    }
}
