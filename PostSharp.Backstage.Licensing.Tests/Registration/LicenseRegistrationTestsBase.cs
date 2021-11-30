using System;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Testing.Services;
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
            return FileSystem.ReadAllLines( LicenseFiles.UserLicenseFile );
        }

        protected void SetStoredLicenseStrings( params string[] licenseStrings )
        {
            FileSystem.Mock.AddFile( LicenseFiles.UserLicenseFile, new MockFileDataEx( licenseStrings ) );
        }

        internal LicenseRegistrationData GetLicenseRegistrationData( string licenseString )
        {
            Assert.True( LicenseFactory.TryCreate( licenseString, out var license ) );
            Assert.True( license!.TryGetLicenseRegistrationData( out var data ) );

            return data!;
        }
    }
}