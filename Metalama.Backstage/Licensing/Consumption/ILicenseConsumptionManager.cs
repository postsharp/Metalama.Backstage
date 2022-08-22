// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption.Sources;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
        /// Returns <c>true</c> when the <paramref name="redistributionLicenseKey"/> is a valid redistribution license key
        /// and the associated license allows to use a redistributed aspect defined in the <paramref name="aspectClassNamespace"/>.
        /// </summary>
        bool ValidateRedistributionLicenseKey( string redistributionLicenseKey, string aspectClassNamespace );

        /// <summary>
        /// Returns maximum aspects count linsesed without namespace restrictions.
        /// </summary>
        int GetNamespaceUnlimitedMaxAspectsCount();

        /// <summary>
        /// Returns <c>true</c> when the <paramref name="consumerNamespace"/> is licensed by a namespace-restricted license.
        /// When <c>true</c> is returned, the <paramref name="maxAspectsCount"/> gives the maximum aspects count allowed
        /// for the <paramref name="licensedNamespace"/> namespace.
        /// </summary>
        /// <remarks>
        /// Having two or more namespace-limited licenses where one restricts a sub-namespace of the other
        /// is not supperted and results can be undeterministic in such case.
        /// </remarks>
        bool TryGetNamespaceLimitedMaxAspectsCount( string consumerNamespace, out int maxAspectsCount, [NotNullWhen( true )] out string? licensedNamespace );
    }
}