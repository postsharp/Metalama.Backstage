// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Generic;

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
        this._licensingConfiguration = services.GetRequiredService<IConfigurationManager>().Get<LicensingConfiguration>();
    }

    protected override IEnumerable<string> GetLicenseStrings() => this._licensingConfiguration.Licenses;
}