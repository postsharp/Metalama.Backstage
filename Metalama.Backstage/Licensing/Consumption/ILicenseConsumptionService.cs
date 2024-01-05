// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Exposes a service to verify the current license and consume features from iut.
    /// </summary>
    [PublicAPI]
    public interface ILicenseConsumptionService : IBackstageService
    {
        // Note that this collection of Messages is a design anomaly. We should have an Initialize method accepting a message sink,
        // or something similar, but adding the initialization step would require more testing.
        
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

        bool IsTrialLicense { get; }

        bool IsRedistributionLicense { get; }

        string? LicenseString { get; }

        event Action? Changed;

        ILicenseConsumptionService WithAdditionalLicense( string? licenseKey );

        ILicenseConsumptionService WithoutLicense();
    }
}