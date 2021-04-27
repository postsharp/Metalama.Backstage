// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing.LicenseServer
{
    public abstract class LicenseLeaseCache
    {
        public abstract void Cache( LicenseLease lease );
    }
}
