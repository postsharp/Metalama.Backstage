// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Consumption.Sources
{
    public class LicenseStringsLicenseSource : LicenseStringsLicenseSourceBase
    {
        private readonly IEnumerable<string> _licenseStrings;
        
        public LicenseStringsLicenseSource( IEnumerable<string> licenseKeys, IServiceProvider services )
            : base( services )
        {
            this._licenseStrings = licenseKeys;
        }

        protected override IEnumerable<string> GetLicenseStrings() => this._licenseStrings;
    }
}