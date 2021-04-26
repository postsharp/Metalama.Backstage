// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing
{
    public class MemoryLicenseSource : ILicenseSource
    {
        private readonly string[] _licenseStrings;

        public string Id { get; }

        public MemoryLicenseSource( string id, params string[] licenseStrings )
        {
            this.Id = id;
            this._licenseStrings = licenseStrings;
        }

        public IEnumerable<string> LicenseStrings => this._licenseStrings;
    }
}
