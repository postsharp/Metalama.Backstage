// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Licensing.Consumption.Sources;

internal class ExplicitLicenseSource : LicenseSourceBase
{
    private readonly string _licenseString;

    public ExplicitLicenseSource( string licenseKey, IServiceProvider services )
        : base( services )
    {
        this._licenseString = licenseKey;
    }

    protected override string? GetLicenseString() => this._licenseString;
}