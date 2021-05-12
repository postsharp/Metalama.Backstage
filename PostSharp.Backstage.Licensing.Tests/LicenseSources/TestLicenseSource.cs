// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Backstage.Licensing.Tests
{
    internal class TestLicenseSource : ILicenseSource, IUsable
    {
        private readonly ILicense[] _licenses;

        public string Id { get; }

        public bool Used { get; set; }

        public TestLicenseSource( string id, params ILicense[] licenses )
        {
            this.Id = id;
            this._licenses = licenses;
        }

        public IEnumerable<ILicense> GetLicenses()
        {
            this.Used = true;
            return this._licenses;
        }
    }
}
