// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Licensing.Consumption;

public record LicensingInitializationOptions
{
    public bool IgnoreUnattendedProcessLicense { get; init; }

    public bool IgnoreUserProfileLicenses { get; init; }

    /// <summary>
    /// Gets the license key that stems from the MSBuild project file.
    /// </summary>
    public string? ProjectLicense { get; init; }
}