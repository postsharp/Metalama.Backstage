// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Consumption.Sources
{
    /// <summary>
    /// License source providing licenses from a license file.
    /// </summary>
    public class FileLicenseSource : LicenseStringsLicenseSourceBase
    {
        private readonly LicensingConfiguration _licensingConfiguration;

        public static FileLicenseSource CreateUserLicenseFileLicenseSource( IServiceProvider services )
        {
            return new FileLicenseSource( services );
        }

        public FileLicenseSource( IServiceProvider services )
            : base( services )
        {
            this._licensingConfiguration = LicensingConfiguration.Load( services );
        }

        protected override IEnumerable<string> GetLicenseStrings() => this._licensingConfiguration.Licenses;
    }
}