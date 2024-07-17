// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using System;

namespace Metalama.Backstage.Licensing.Licenses
{
    public partial class LicenseKeyData
    {
        /// <exclude/>
        public bool VerifySignature()
        {
            if ( !this.RequiresSignature )
            {
                return true;
            }

            if ( !this.SignatureKeyId.HasValue )
            {
                return false;
            }

            var publicKey = LicenseCryptography.GetPublicKey( this.SignatureKeyId.Value );

            try
            {
                return RetryHelper.Retry( () => this.VerifySignature( publicKey ), _ => true, this._logger );
            }
            catch ( Exception e )
            {
                this._logger.Error?.Log( $"Signature verification of license key '{this.LicenseString}' failed: {e}" );
                
                return false;
            }
        }

        private static bool ComparePublicKeyToken( byte[]? key1, byte[]? key2 )
        {
            if ( key1 == null )
            {
                return key2 == null;
            }

            if ( key2 == null )
            {
                return false;
            }

            for ( var i = 0; i < key1.Length; i++ )
            {
                if ( key1[i] != key2[i] )
                {
                    return false;
                }
            }

            return true;
        }
    }
}