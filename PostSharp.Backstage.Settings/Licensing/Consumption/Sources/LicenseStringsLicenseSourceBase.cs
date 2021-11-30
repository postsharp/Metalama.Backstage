// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Consumption.Sources
{
    public abstract class LicenseStringsLicenseSourceBase : ILicenseSource
    {
        private readonly IServiceProvider _services;
        private readonly ILogger? _logger;

        protected LicenseStringsLicenseSourceBase( IServiceProvider services )
        {
            _services = services;

            _logger = services.GetOptionalTraceLogger<LicenseStringsLicenseSourceBase>();
        }

        protected abstract IEnumerable<string> GetLicenseStrings();

        public IEnumerable<ILicense> GetLicenses()
        {
            var licenseFactory = new LicenseFactory( _services );

            foreach (var licenseString in GetLicenseStrings())
            {
                if (string.IsNullOrWhiteSpace( licenseString ))
                {
                    continue;
                }

                if (licenseFactory.TryCreate( licenseString, out var license ))
                {
                    yield return license;
                }
            }
        }
    }
}