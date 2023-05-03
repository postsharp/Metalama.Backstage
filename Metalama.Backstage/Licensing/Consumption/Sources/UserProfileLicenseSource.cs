// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Licensing.Consumption.Sources;

/// <summary>
/// License source providing licenses from a license file.
/// </summary>
internal class UserProfileLicenseSource : LicenseSourceBase
{
    private readonly IConfigurationManager _configurationManager;
    private LicensingConfiguration _licensingConfiguration;

    public UserProfileLicenseSource( IServiceProvider services )
        : base( services )
    {
        this._configurationManager = services.GetRequiredBackstageService<IConfigurationManager>();
        this._licensingConfiguration = this._configurationManager.Get<LicensingConfiguration>();
        this._configurationManager.ConfigurationFileChanged += this.OnConfigurationFileChanged;
    }

    private void OnConfigurationFileChanged( ConfigurationFile file )
    {
        if ( file is LicensingConfiguration licensingConfiguration )
        {
            this._licensingConfiguration = licensingConfiguration;
            this.OnChanged();
        }
    }

    protected override string? GetLicenseString() => this._licensingConfiguration.License;
}