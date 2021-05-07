// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Testing;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests
{
    public abstract class LicensingTestsBase : TestsBase
    {
        private protected LicenseFactory LicenseFactory { get; }

        private protected SelfSignedLicenseFactory SelfSignedLicenseFactory { get; }

        public LicensingTestsBase( ITestOutputHelper logger )
            : base( logger )
        {
            this.LicenseFactory = new( this.Services, this.Trace );
            this.SelfSignedLicenseFactory = new( this.Services );
        }
    }
}
