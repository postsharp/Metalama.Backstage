// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Testing.Services;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Registration
{
    public abstract class LicenseRegistrationTestsBase : LicensingTestsBase
    {
        private protected LicenseRegistrationTestsBase(
            ITestOutputHelper logger,
            Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base(
                logger,
                serviceCollection => serviceBuilder?.Invoke( serviceCollection ) ) { }

        protected string[] ReadStoredLicenseStrings()
            => this.ServiceProvider.GetRequiredService<IConfigurationManager>().Get<LicensingConfiguration>().Licenses;

        protected void SetStoredLicenseStrings( params string[] licenseStrings )
        {
            var configuration = new LicensingConfiguration { Licenses = licenseStrings };

            this.FileSystem.Mock.AddFile(
                this.LicensingConfigurationFile,
                new MockFileDataEx( configuration.ToJson() ) );
        }

        internal LicenseRegistrationData GetLicenseRegistrationData( string licenseString )
        {
            Assert.True( this.LicenseFactory.TryCreate( licenseString, out var license ) );
            Assert.True( license!.TryGetLicenseRegistrationData( out var data ) );

            return data!;
        }
    }
}