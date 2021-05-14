// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
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

        private protected TestDiagnosticsSink Diagnostics { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public LicensingTestsBase( ITestOutputHelper logger )
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            : base( logger )
        {
            this.LicenseFactory = new( this.Services );
            this.SelfSignedLicenseFactory = new( this.Services );
        }

        protected override IServiceCollection SetUpServices( IServiceCollection serviceCollection )
        {
            this.Diagnostics = new( this.LoggerFactory, "default" );

            return base.SetUpServices( serviceCollection )
                .AddSingleton<IDiagnosticsSink>( this.Diagnostics )
                .AddSingleton<IApplicationInfo>( new TestApplicationInfo( false, new( 0, 1, 0 ), new( 2021, 1, 1 ) ) );
        }
    }
}
