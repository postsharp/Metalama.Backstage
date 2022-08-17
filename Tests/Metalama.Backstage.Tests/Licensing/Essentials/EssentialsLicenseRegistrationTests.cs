// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Licensing.Registration.Essentials;
using Metalama.Backstage.Licensing.Tests.Licensing.Registration;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Essentials
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

#pragma warning disable CA1307 // Method does not exist in .NET Standard.
            Assert.Single(
                this.Log.LogEntries,
                x => x.Message != null && x.Message.Contains( "Failed to register Essentials license: An Essentials license is registered already." ) );
#pragma warning restore CA1307
        }
    }
}