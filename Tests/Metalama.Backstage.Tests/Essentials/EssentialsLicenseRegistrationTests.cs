// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration.Essentials;
using Metalama.Backstage.Licensing.Tests.Registration;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Essentials
{
    public class EssentialsLicenseRegistrationTests : LicenseRegistrationTestsBase
    {
        private readonly EssentialsLicenseRegistrar _registrar;

        public EssentialsLicenseRegistrationTests( ITestOutputHelper logger )
            : base( logger )
        {
            this._registrar = new EssentialsLicenseRegistrar( this.ServiceProvider );
        }

        private void AssertSingleEssentialsLicenseRegistered()
        {
            var registeredLicenseString = this.ReadStoredLicenseStrings().Single();
            Assert.True( this.LicenseFactory.TryCreate( registeredLicenseString, out var registeredLicense ) );
            Assert.True( registeredLicense!.TryGetLicenseRegistrationData( out var data ) );
            Assert.True( Guid.TryParse( data!.UniqueId, out var id ) );
            Assert.NotEqual( Guid.Empty, id );
            Assert.Equal( LicenseType.Essentials, data.LicenseType );
        }

        [Fact]
        public void EssentialsLicenseRegistersInCleanEnvironment()
        {
            Assert.True( this._registrar.TryRegisterLicense() );
            this.AssertSingleEssentialsLicenseRegistered();
        }

        [Fact]
        public void RepeatedEssentialsLicenseRegistrationKeepsSingleLicenseRegistered()
        {
            Assert.True( this._registrar.TryRegisterLicense() );
            Assert.True( this._registrar.TryRegisterLicense() );
            this.AssertSingleEssentialsLicenseRegistered();

            Assert.Single(
                this.Log.LogEntries,
                x => x.Message.Contains( "Failed to register Essentials license: A Essentials license is registered already." ) );
        }
    }
}