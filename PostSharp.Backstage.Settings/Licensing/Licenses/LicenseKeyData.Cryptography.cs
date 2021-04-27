// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal partial class LicenseKeyData
    {
        /// <exclude/>
        public bool VerifySignature()
        {
            if ( !this.RequiresSignature )
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
    }
}
