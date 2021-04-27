// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using PostSharp.Backstage.Licensing.Sources;

namespace PostSharp.Backstage.Licensing.Tests
{
    public class TestLicenseSource : ILicenseSource
    {
        private readonly string[] _licenseStrings;

        public string Id { get; }

        public TestLicenseSource( string id, params string[] licenseStrings )
        {
            this.Id = id;
            this._licenseStrings = licenseStrings;
        }

        public IEnumerable<string> LicenseStrings => this._licenseStrings;
    }
}
