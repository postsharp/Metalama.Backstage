// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Testing;
using PostSharp.Backstage.Testing.Services;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests
{
    public abstract class LicensingTestsBase : TestsBase
    {
        private protected LicenseFactory LicenseFactory { get; }

        private protected UnsignedLicenseFactory SelfSignedLicenseFactory { get; }

        private protected TestDiagnosticsSink Diagnostics { get; }

        public LicensingTestsBase( ITestOutputHelper logger )
            : base( logger )
        {
            this.Diagnostics = new( this.Trace, "default" );
            this.Services.SetService<IDiagnosticsSink>( this.Diagnostics );
            this.Services.SetService<IApplicationInfo>( new ApplicationInfo( false, new( 0, 1, 0 ), new( 2021, 1, 1 ) ) );

            this.LicenseFactory = new( this.Services );
            this.SelfSignedLicenseFactory = new( this.Services );
        }
    }
}
