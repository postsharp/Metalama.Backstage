// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;

namespace Metalama.Backstage.Licensing.Consumption;

[PublicAPI]
public record LicensingInitializationOptions
{
    public bool IgnoreUnattendedProcessLicense { get; init; }

    public bool IgnoreUserProfileLicenses { get; init; }

    /// <summary>
    /// Gets the license key that stems from the MSBuild project file.
    /// </summary>
    public string? ProjectLicense { get; init; }

    /// <summary>
    /// Gets a value indicating whether license audit should be disabled. It is convenient to disable it during tests to avoid poisoning of the server.
    /// </summary>
    public bool DisableLicenseAudit { get; init; }
}