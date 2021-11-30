﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Consumption.Sources
{
    public class LicenseStringsLicenseSource : LicenseStringsLicenseSourceBase
    {
        private readonly IEnumerable<string> _licenseStrings;
        private readonly IServiceProvider _services;
        private readonly ILogger? _logger;

        public LicenseStringsLicenseSource( IEnumerable<string> licenseKeys, IServiceProvider services )
            : base( services )
        {
            _licenseStrings = licenseKeys;
            _services = services;

            _logger = services.GetOptionalTraceLogger<LicenseStringsLicenseSource>();
        }

        protected override IEnumerable<string> GetLicenseStrings()
        {
            return _licenseStrings;
        }
    }
}