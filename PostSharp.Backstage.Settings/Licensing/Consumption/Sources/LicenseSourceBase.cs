// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Consumption.Sources
{
    public abstract class LicenseSourceBase : ILicenseSource
    {
        private readonly IServiceProvider _services;

        protected LicenseSourceBase( IServiceProvider services )
        {
            this._services = services;
        }

        protected abstract IEnumerable<string> GetLicenseStrings();

        public IEnumerable<ILicense> GetLicenses( Action<LicensingMessage> reportMessage )
        {
            var licenseFactory = new LicenseFactory( this._services );

            foreach ( var licenseString in this.GetLicenseStrings() )
            {
                if ( string.IsNullOrWhiteSpace( licenseString ) )
                {
                    continue;
                }

                if ( licenseFactory.TryCreate( licenseString, out var license ) )
                {
                    yield return license;
                }
            }
        }

        public override string ToString() => this.GetType().Name;
    }
}