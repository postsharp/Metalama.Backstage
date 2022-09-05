// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

            if ( publicKey == null )
            {
                return false;
            }

            try
            {
                return this.VerifySignature( publicKey );
            }
            catch
            {
                return false;
            }
        }

        private static bool ComparePublicKeyToken( byte[] key1, byte[] key2 )
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