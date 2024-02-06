// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Testing;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing
{
    public abstract class LicensingTestsBase : TestsBase
    {
        private protected LicenseFactory LicenseFactory { get; }

        private protected LicensingTestsBase(
            ITestOutputHelper logger,
            Action<ServiceProviderBuilder>? serviceBuilder = null,
            bool initializeConfiguration = true,
            bool isTelemetryEnabled = false )
            : base(
                logger,
                initializeConfiguration
                    ? services =>
                    {
                        // ReSharper disable once ExplicitCallerInfoArgument
                        services
                            .AddSingleton<IApplicationInfoProvider>(
                                new ApplicationInfoProvider(
                                    new TestApplicationInfo(
                                        "Licensing Test App",
                                        false,
                                        "1.0",
                                        new DateTime( 2021, 1, 1 ) ) { IsTelemetryEnabled = isTelemetryEnabled } ) )
                            .AddConfigurationManager();

                        serviceBuilder?.Invoke( services );
                    }
                    : null )
        {
            this.LicenseFactory = new LicenseFactory( this.ServiceProvider );
        }

        protected string? ReadStoredLicenseString() => TestLicensingConfigurationHelpers.ReadStoredLicenseString( this.ServiceProvider );

        protected void SetStoredLicenseString( string licenseString )
            => TestLicensingConfigurationHelpers.SetStoredLicenseString( this.ServiceProvider, licenseString );

        internal LicenseRegistrationData GetLicenseRegistrationData( string licenseString )
        {
            Assert.True( this.LicenseFactory.TryCreate( licenseString, out var license, out var errorMessage ) );
            Assert.Null( errorMessage );
            Assert.True( license!.TryGetLicenseRegistrationData( out var data, out errorMessage ) );
            Assert.Null( errorMessage );

            return data!;
        }
    }
}