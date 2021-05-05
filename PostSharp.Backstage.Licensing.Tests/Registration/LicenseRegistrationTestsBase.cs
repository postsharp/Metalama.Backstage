﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Registration;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Registration
{
    public abstract class LicenseRegistrationTestsBase : LicensingTestsBase
    {
        protected LicenseRegistrationTestsBase( ITestOutputHelper logger )
            : base( logger )
        {
        }

        internal LicenseRegistrationData GetLicenseRegistrationData( string licenseString )
        {
            Assert.True( this.LicenseFactory.TryCreate( licenseString, out var license ) );
            Assert.True( license!.TryGetLicenseRegistrationData( out var data ) );
            return data!;
        }
    }
}