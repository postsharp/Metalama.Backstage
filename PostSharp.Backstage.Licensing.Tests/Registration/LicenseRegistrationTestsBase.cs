﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Testing.Services;
using System;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Registration
{
    public abstract class LicenseRegistrationTestsBase : LicensingTestsBase
    {
        private protected LicenseRegistrationTestsBase(
            ITestOutputHelper logger,
            Action<BackstageServiceCollection>? serviceBuilder = null )
            : base(
                logger,
                serviceCollection => serviceBuilder?.Invoke( serviceCollection ) ) { }

        protected string[] ReadStoredLicenseStrings()
        {
            return this.FileSystem.ReadAllLines( this.LicenseFiles.UserLicenseFile );
        }

        protected void SetStoredLicenseStrings( params string[] licenseStrings )
        {
            this.FileSystem.Mock.AddFile( this.LicenseFiles.UserLicenseFile, new MockFileDataEx( licenseStrings ) );
        }

        internal LicenseRegistrationData GetLicenseRegistrationData( string licenseString )
        {
            Assert.True( this.LicenseFactory.TryCreate( licenseString, out var license ) );
            Assert.True( license!.TryGetLicenseRegistrationData( out var data ) );

            return data!;
        }
    }
}