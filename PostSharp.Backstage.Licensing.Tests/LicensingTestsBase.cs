// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Tests.Services;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests
{
    public abstract class LicensingTestsBase
    {
        private protected ITrace Trace { get; }

        private protected TestServices Services { get; } = new();

        private protected LicenseFactory LicenseFactory { get; }

        public LicensingTestsBase( ITestOutputHelper logger )
        {
            this.Trace = new TestTrace( logger );
            this.LicenseFactory = new( this.Services, this.Trace );
        }
    }
}
