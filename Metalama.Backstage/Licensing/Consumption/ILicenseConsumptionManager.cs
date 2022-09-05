// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Manages license consumption.
    /// </summary>
    public interface ILicenseConsumptionManager : IBackstageService
    {
        /// <summary>
        /// Gets redistribution license key if available.
        /// </summary>
        /// <remarks>
        /// This license key is to be embedded into an output assembly.
        /// </remarks>
        string? RedistributionLicenseKey { get; }

        /// <summary>
        /// Gets the list of licensing messages that have been emitted when calling <see cref="CanConsume"/> or when initializing the component.
        /// </summary>
        IReadOnlyList<LicensingMessage> Messages { get; }

        /// <summary>
        /// Provides information about availability of <paramref name="requirement"/>.
        /// </summary>
        /// <param name="requirement">The required license requirement.</param>
        /// <param name="consumerNamespace">The consuming namespace, or <c>null</c> if this is a global feature.</param>
        /// <returns>A value indicating if the <paramref name="requirement"/> is available.</returns>
        bool CanConsume( LicenseRequirement requirement, string? consumerNamespace = null );

        /// <summary>
        /// Returns <c>true</c> when the <paramref name="redistributionLicenseKey"/> is a valid redistribution license key
        /// and the associated license allows to use a redistributed aspect defined in the <paramref name="aspectClassNamespace"/>.
        /// </summary>
        /// <remarks>
        /// This method serves for validation of redistribution license keys embedded in referenced aspect libraries.
        /// </remarks>
        bool ValidateRedistributionLicenseKey( string redistributionLicenseKey, string aspectClassNamespace );
    }
}