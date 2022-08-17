// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Tests.Licensing.LicenseSources
{
    internal class TestLicenseSource : ILicenseSource, IUsable
    {
        private readonly ILicense[] _licenses;

        public string Id { get; }

        public bool IsRedistributionLicenseSource { get; }

        public bool IsUsed { get; set; }

        public TestLicenseSource( string id, bool isRedistributionLicenseSource, params ILicense[] licenses )
        {
            this.Id = id;
            this._licenses = licenses;
            this.IsRedistributionLicenseSource = isRedistributionLicenseSource;
        }

        public IEnumerable<ILicense> GetLicenses( Action<LicensingMessage> reportWarning )
        {
            this.IsUsed = true;

            return this._licenses;
        }
    }
}