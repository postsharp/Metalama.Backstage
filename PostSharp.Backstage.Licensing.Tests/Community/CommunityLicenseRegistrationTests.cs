// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration.Community;
using PostSharp.Backstage.Licensing.Tests.Registration;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Community
{
    public class CommunityLicenseRegistrationTests : LicenseRegistrationTestsBase
    {
        private readonly CommunityLicenseRegistrar _registrar;

        public CommunityLicenseRegistrationTests( ITestOutputHelper logger )
            : base( logger )
        {
            this._registrar = new( this.Services );
        }

        private void AssertSingleCommunityLicenseRegistered()
        {
            var registeredLicenseString = this.ReadStoredLicenseStrings().Single();
            Assert.True( this.LicenseFactory.TryCreate( registeredLicenseString, out var registeredLicense ) );
            Assert.True( registeredLicense!.TryGetLicenseRegistrationData( out var data ) );
            Assert.True( Guid.TryParse( data!.UniqueId, out var id ) );
            Assert.NotEqual( Guid.Empty, id );
            Assert.Equal( LicenseType.Community, data.LicenseType );
        }

        [Fact]
        public void CommunityLicenseRegistersInCleanEnvironment()
        {
            Assert.True( this._registrar.TryRegisterLicense() );
            this.AssertSingleCommunityLicenseRegistered();
        }

        [Fact]
        public void RepeatedCommunityLicenseRegisterationKeepsSingleLicenseRegistered()
        {
            Assert.True( this._registrar.TryRegisterLicense() );
            Assert.True( this._registrar.TryRegisterLicense() );
            this.AssertSingleCommunityLicenseRegistered();
            Assert.Single( this.Log.LogEntries, x => x.Message == "Failed to register community license: A community license is registered already." );
        }
    }
}
