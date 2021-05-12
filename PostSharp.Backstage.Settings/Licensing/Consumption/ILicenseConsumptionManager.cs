// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Manages license consumption.
    /// </summary>
    public interface ILicenseConsumptionManager
    {
        /// <summary>
        /// Provides information about availability of <paramref name="requiredFeatures"/> for a <paramref name="consumer"/>.
        /// </summary>
        /// <param name="consumer">License consumer requesting licensed features.</param>
        /// <param name="requiredFeatures">The requested features.</param>
        /// <returns>A value indicating if the <paramref name="requiredFeatures"/> is available to the <paramref name="consumer" />.</returns>
        public bool CanConsumeFeature( ILicenseConsumer consumer, LicensedFeatures requiredFeatures );

        // TODO Behavior?
        public void ConsumeFeature( ILicenseConsumer consumer, LicensedFeatures requiredFeatures );
    }
}
