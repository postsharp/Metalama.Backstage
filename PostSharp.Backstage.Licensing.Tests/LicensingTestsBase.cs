// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Tests.Services;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests
{
    public abstract class LicensingTestsBase
    {
        private protected TestTrace Trace { get; }

        private protected TestServices Services { get; }

        private protected LicenseFactory LicenseFactory { get; }

        public LicensingTestsBase( ITestOutputHelper logger )
        {
            this.Trace = new( logger );
            this.Services = new TestServices( this.Trace );
            this.LicenseFactory = new( this.Services, this.Trace );
        }
    }
}
