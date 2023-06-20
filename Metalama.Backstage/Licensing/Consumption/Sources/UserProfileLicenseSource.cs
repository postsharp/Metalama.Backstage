﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Licensing.Consumption.Sources;

/// <summary>
/// License source providing licenses from a license file.
/// </summary>
internal class UserProfileLicenseSource : LicenseSourceBase
{
    private readonly LicensingConfiguration _licensingConfiguration;

    public UserProfileLicenseSource( IServiceProvider services )
        : base( services )
    {
        this._licensingConfiguration = services.GetRequiredBackstageService<IConfigurationManager>().Get<LicensingConfiguration>();
    }

    protected override string? GetLicenseString() => this._licensingConfiguration.License;

    public override string GetDescription() => "user profile";
}