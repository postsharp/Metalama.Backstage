// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Registration.Free;
using Metalama.Backstage.Licensing.Tests.Licensing.Registration;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Free
{
    public class FreeLicenseRegistrationTests : LicenseRegistrationTestsBase
    {
        private readonly FreeLicenseRegistrar _registrar;

        public FreeLicenseRegistrationTests( ITestOutputHelper logger )
            : base( logger )
        {
            this._registrar = new FreeLicenseRegistrar( this.ServiceProvider );
        }

        private void AssertSingleFreeLicenseRegistered()
        {
            var registeredLicenseString = this.ReadStoredLicenseStrings().Single();
            Assert.True( this.LicenseFactory.TryCreate( registeredLicenseString, out var registeredLicense ) );
            Assert.True( registeredLicense!.TryGetLicenseRegistrationData( out var data ) );
            Assert.True( Guid.TryParse( data!.UniqueId, out var id ) );
            Assert.NotEqual( Guid.Empty, id );
            Assert.Equal( LicensedProduct.MetalamaFree, data.Product );
        }

        [Fact]
        public void FreeLicenseRegistersInCleanEnvironment()
        {
            Assert.True( this._registrar.TryRegisterLicense() );
            this.AssertSingleFreeLicenseRegistered();
        }

        [Fact]
        public void RepeatedFreeLicenseRegistrationKeepsSingleLicenseRegistered()
        {
            Assert.True( this._registrar.TryRegisterLicense() );
            Assert.True( this._registrar.TryRegisterLicense() );
            this.AssertSingleFreeLicenseRegistered();

#pragma warning disable SA1001, SA1111, SA1113, SA1115, SA1116
            Assert.Single(
                this.Log.LogEntries,
                x => x.Message != null && x.Message.Contains( "Failed to register Metalama Free license: A Metalama Free license is registered already."
#if NET
                , StringComparison.InvariantCulture
#endif
                ) );
#pragma warning restore SA1001, SA1111, SA1113, SA1115, SA1116
        }
    }
}