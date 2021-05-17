// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
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

        private protected IStandardLicenseFileLocations LicenseFiles { get; }

        public LicensingTestsBase( ITestOutputHelper logger, Action<IServiceCollection>? serviceBuilder = null )
            : base(
                  logger,
                  serviceCollection =>
                  {
                      serviceCollection
                          .AddSingleton<IDiagnosticsSink>( services => new TestDiagnosticsSink( services, "default" ) )
                          .AddSingleton<IApplicationInfo>( new TestApplicationInfo( false, new( 0, 1, 0 ), new( 2021, 1, 1 ) ) )
                          .AddStandardLicenseFilesLocations();
                      serviceBuilder?.Invoke( serviceCollection );
                  } )
        {
            this.LicenseFactory = new( this.Services );
            this.SelfSignedLicenseFactory = new( this.Services );
            this.Diagnostics = (TestDiagnosticsSink) this.Services.GetRequiredService<IDiagnosticsSink>();
            this.LicenseFiles = this.Services.GetRequiredService<IStandardLicenseFileLocations>();
        }
    }
}
