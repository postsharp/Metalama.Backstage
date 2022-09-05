// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption.Sources;

internal class ExplicitLicenseSource : LicenseSourceBase
{
    private readonly IEnumerable<string> _licenseStrings;

    public ExplicitLicenseSource( IEnumerable<string> licenseKeys, IServiceProvider services )
        : base( services )
    {
        this._licenseStrings = licenseKeys;
    }

    protected override IEnumerable<string> GetLicenseStrings() => this._licenseStrings;
}