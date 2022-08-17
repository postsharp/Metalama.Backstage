// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption.Sources;

internal class ExplicitLicenseSource : LicenseSourceBase
{
    private readonly IEnumerable<string> _licenseStrings;

    public ExplicitLicenseSource( IEnumerable<string> licenseKeys, bool isRedistributionLicenseSource, IServiceProvider services )
        : base( services )
    {
        this._licenseStrings = licenseKeys;
        this.IsRedistributionLicenseSource = isRedistributionLicenseSource;
    }

    public override bool IsRedistributionLicenseSource { get; }

    protected override IEnumerable<string> GetLicenseStrings() => this._licenseStrings;
}