// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing
{
    public interface ILicenseConsumptionManager
    {
        public bool CanConsumeFeature( ILicenseConsumer consumer, LicensedFeatures requiredFeatures );

        public void ConsumeFeature( ILicenseConsumer consumer, LicensedFeatures requiredFeatures );
    }
}
