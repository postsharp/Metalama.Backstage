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
        public bool CanConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures );

        /// <summary>
        /// If the <paramref name="requiredFeatures"/> cannot be consumed by the <paramref name="consumer"/>,
        /// an error diagnostic is emitted in the <paramref name="consumer"/>'s diagnostics sink.
        /// </summary>
        /// <param name="consumer">License consumer requesting licensed features.</param>
        /// <param name="requiredFeatures">The requested features.</param>
        public void ConsumeFeatures( ILicenseConsumer consumer, LicensedFeatures requiredFeatures );
    }
}
