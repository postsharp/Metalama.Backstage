// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Configuration;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Testing;
using PostSharp.Backstage.Testing.Services;
using System;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests
{
    public abstract class LicensingTestsBase : TestsBase
    {
        private protected LicenseFactory LicenseFactory { get; }

        protected string LicensingConfigurationFile { get; }

        private protected UnsignedLicenseFactory SelfSignedLicenseFactory { get; }

        private protected LicensingTestsBase(
            ITestOutputHelper logger,
            Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base(
                logger,
                services =>
                {
                    // ReSharper disable once ExplicitCallerInfoArgument
                    services
                        .AddSingleton<IApplicationInfo>(
                            new TestApplicationInfo(
                                "Licensing Test App",
                                false,
                                "1.0",
                                new DateTime( 2021, 1, 1 ) ) )
                        .AddConfigurationManager();

                    serviceBuilder?.Invoke( services );
                } )
        {
            this.LicenseFactory = new LicenseFactory( this.Services );
            this.SelfSignedLicenseFactory = new UnsignedLicenseFactory( this.Services );
            this.LicensingConfigurationFile = this.Services.GetRequiredService<IConfigurationManager>().GetFileName<LicensingConfiguration>();
        }
    }
}