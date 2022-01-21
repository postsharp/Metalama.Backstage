// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Registration;
using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Backstage.Licensing.Licenses
{
    /// <summary>
    /// A license providing licensed features.
    /// </summary>
    /// <remarks>
    /// Reports warnings using <see cref="IBackstageDiagnosticSink" />.
    /// </remarks>
    public interface ILicense
    {
        /// <summary>
        /// Tries to retrieves or deserialize and validate license data relevant to license consumption.
        /// The data is either deserialized (e.g. from a license key) or retrieved from a license provider (e.g. license server.)
        /// </summary>
        /// <param name="licenseConsumptionData">The license data relevant to license consumption.</param>
        /// <returns>
        /// <c>true</c> if the object represents or retrieves a consistent and valid license.
        /// </returns>
        bool TryGetLicenseConsumptionData( [MaybeNullWhen( false )] out LicenseConsumptionData licenseConsumptionData );

        /// <summary>
        /// Tries to deserialize data relevant to license registration.
        /// </summary>
        /// <param name="licenseRegistrationData">The license data relevant to license registration.</param>
        /// <returns>
        /// <c>true</c> if the object represents a consistent license.
        /// </returns>
        bool TryGetLicenseRegistrationData(
            [MaybeNullWhen( false )] out LicenseRegistrationData licenseRegistrationData );
    }
}