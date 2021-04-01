// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing
{
    internal abstract class LicenseeIdentifier
    {
        /// <exclude/>
        public abstract long GetCurrentMachineHash();

        internal long GetCurrentUserHash()
        {
            return CryptoUtilities.ComputeStringHash64( Environment.UserName );
        }
    }
}
