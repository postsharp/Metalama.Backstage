// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption.Sources;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Manages license consumption.
    /// </summary>
    public interface ILicenseConsumptionManager : IBackstageService
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

        /// <summary>
        /// Gets all redistribution license keys from all license sources marked as <see cref="ILicenseSource.IsRedistributionLicenseSource" />.
        /// </summary>
        IEnumerable<string> GetRedistributionLicenseKeys();

        /// <summary>
        /// Returns <c>true</c> when the provided <paramref name="redistributionLicenseKey"/> is a valid redistribution license key.
        /// </summary>
        bool ValidateRedistributionLicenseKey( string redistributionLicenseKey );

        /// <summary>
        /// Returns maximum aspects count linsesed for the given namespace.
        /// </summary>
        /// <remarks>
        /// When namespace is not provided, namespace-restricted licenses are not considered.
        /// </remarks>
        int GetMaxAspectsCount( string? consumerNamespace = null );
    }
}