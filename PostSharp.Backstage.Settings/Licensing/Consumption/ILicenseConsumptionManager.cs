// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Manages license consumption.
    /// </summary>
    public interface ILicenseConsumptionManager
    {
        /// <summary>
        /// Provides information about availability of <paramref name="requiredFeatures"/>.
        /// </summary>
        /// <param name="requiredFeatures">The requested features.</param>
        /// <param name="consumerNamespace">The consuming namespace, or <c>null</c> if this is a global feature.</param>
        /// <returns>A value indicating if the <paramref name="requiredFeatures"/> is available.</returns>
        bool CanConsumeFeatures( LicensedFeatures requiredFeatures, string? consumerNamespace = null );
        
        /// <summary>
        /// Gets the list of licensing messages that have been emitted when calling <see cref="CanConsumeFeatures"/> or when initializing the component.
        /// </summary>
        IReadOnlyList<LicensingMessage> Messages { get; }
    }
}