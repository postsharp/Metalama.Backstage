// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption.Sources
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