// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Registration.Community;
using PostSharp.Backstage.Licensing.Tests.Registration;
using System;
using System.Linq;
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
            _registrar = new CommunityLicenseRegistrar( Services );
        }

        private void AssertSingleCommunityLicenseRegistered()
        {
            var registeredLicenseString = ReadStoredLicenseStrings().Single();
            Assert.True( LicenseFactory.TryCreate( registeredLicenseString, out var registeredLicense ) );
            Assert.True( registeredLicense!.TryGetLicenseRegistrationData( out var data ) );
            Assert.True( Guid.TryParse( data!.UniqueId, out var id ) );
            Assert.NotEqual( Guid.Empty, id );
            Assert.Equal( LicenseType.Community, data.LicenseType );
        }

        [Fact]
        public void CommunityLicenseRegistersInCleanEnvironment()
        {
            Assert.True( _registrar.TryRegisterLicense() );
            AssertSingleCommunityLicenseRegistered();
        }

        [Fact]
        public void RepeatedCommunityLicenseRegistrationKeepsSingleLicenseRegistered()
        {
            Assert.True( _registrar.TryRegisterLicense() );
            Assert.True( _registrar.TryRegisterLicense() );
            AssertSingleCommunityLicenseRegistered();
            Assert.Single( Log.LogEntries, x => x.Message == "Failed to register community license: A community license is registered already." );
        }
    }
}