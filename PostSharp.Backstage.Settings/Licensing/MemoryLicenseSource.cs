// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing
{
    public class MemoryLicenseSource : ILicenseSource
    {
        private readonly string[] _licenseKeys;

        public string Id { get; }

        public MemoryLicenseSource( string id, params string[] licenseKeys )
        {
            this.Id = id;
            this._licenseKeys = licenseKeys;
        }

        public IEnumerable<string> LicenseKeys => this._licenseKeys;
    }
}
