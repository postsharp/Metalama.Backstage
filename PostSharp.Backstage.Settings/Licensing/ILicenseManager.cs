// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing
{
    public interface ILicenseManager
    {
        public bool CanUseFeature( LicensedFeatures features );

        public void UseFeature( LicensedFeatures features );
    }
}
