﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using System;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing
{
    public abstract class LicensingTestsBase : TestsBase
    {
        private protected LicenseFactory LicenseFactory { get; }

        protected string LicensingConfigurationFile { get; }

        private protected UnsignedLicenseFactory UnsignedLicenseFactory { get; }

        private protected LicensingTestsBase(
            ITestOutputHelper logger,
            Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base(
                logger,
                services =>
                {
                    // ReSharper disable once ExplicitCallerInfoArgument
                    services
                        .AddSingleton<IApplicationInfoProvider>(
                            new ApplicationInfoProvider(
                                new TestApplicationInfo(
                                    "Licensing Test App",
                                    false,
                                    "1.0",
                                    new DateTime( 2021, 1, 1 ) ) ) )
                        .AddConfigurationManager();

                    serviceBuilder?.Invoke( services );
                } )
        {
            this.LicenseFactory = new LicenseFactory( this.ServiceProvider );
            this.UnsignedLicenseFactory = new UnsignedLicenseFactory( this.ServiceProvider );
            this.LicensingConfigurationFile = this.ServiceProvider.GetRequiredBackstageService<IConfigurationManager>().GetFileName<LicensingConfiguration>();
        }
    }
}