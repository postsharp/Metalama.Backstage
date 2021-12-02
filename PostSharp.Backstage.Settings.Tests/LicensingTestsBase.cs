// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Testing;
using PostSharp.Backstage.Testing.Services;
using System;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests
{
    public abstract class LicensingTestsBase : TestsBase
    {
        private protected LicenseFactory LicenseFactory { get; }

        private protected UnsignedLicenseFactory SelfSignedLicenseFactory { get; }

        private protected TestDiagnosticsSink Diagnostics { get; }

        private protected IStandardLicenseFileLocations LicenseFiles { get; }

        private protected LicensingTestsBase(
            ITestOutputHelper logger,
            Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base(
                logger,
                services =>
                {
                    // ReSharper disable once ExplicitCallerInfoArgument
                    services.AddSingleton<IBackstageDiagnosticSink>( new TestDiagnosticsSink( services.ServiceProvider, "default" ) );

                    services.AddSingleton<IApplicationInfo>(
                        new TestApplicationInfo(
                            "Licensing Test App",
                            false,
                            new Version( 0, 1, 0 ),
                            new DateTime( 2021, 1, 1 ) ) );

                    services.AddStandardLicenseFilesLocations();

                    serviceBuilder?.Invoke( services );
                } )
        {
            this.LicenseFactory = new LicenseFactory( this.Services );
            this.SelfSignedLicenseFactory = new UnsignedLicenseFactory( this.Services );
            this.Diagnostics = (TestDiagnosticsSink) this.Services.GetRequiredService<IBackstageDiagnosticSink>();
            this.LicenseFiles = this.Services.GetRequiredService<IStandardLicenseFileLocations>();
        }
    }
}