// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Consumption
{
    public class SingleLicenseKeyTests : LicenseConsumptionManagerTestsBase
    {
        public SingleLicenseKeyTests( ITestOutputHelper logger )
            : base( logger )
        {
        }

        private void TestOneLicense( string licenseKey, LicensedFeatures requiredFeatures, bool expectedCanConsume )
        {
            this.TestOneLicense( licenseKey, requiredFeatures, reuqiredNamespace: "Foo", expectedCanConsume );
        }

        private void TestOneLicense( string licenseKey, LicensedFeatures requiredFeatures, string reuqiredNamespace, bool expectedCanConsume )
        {
            var license = this.CreateLicense( licenseKey );
            var manager = this.CreateConsumptionManager( license );
            this.TestConsumption( manager, requiredFeatures, reuqiredNamespace, expectedCanConsume );
            Assert.NotEqual( expectedCanConsume, this.AutoRegistrar.RegistrationAttempted );
        }

        [Fact]
        public void NoLicenseAutoRegistersEvaluationLicense()
        {
            LicenseConsumptionManager manager = new( this.Services, this.Trace );
            this.TestConsumption( manager, LicensedFeatures.Caravela, false );
            Assert.True( this.AutoRegistrar.RegistrationAttempted );
        }

        [Fact]
        public void UltimateLicenseAllowsCaravelaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Ultimate, LicensedFeatures.Caravela, true );
        }

        [Fact]
        public void LoggingLicenseForbidsCaravelaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Logging, LicensedFeatures.Caravela, false );
        }

        [Fact]
        public void OpenSourceAllowsCaravelaInSameNamespace()
        {
            this.TestOneLicense( TestLicenseKeys.OpenSource, LicensedFeatures.Caravela, TestLicenseKeys.OpenSourceNamespace, true );
        }

        [Fact]
        public void OpenSourceForbidsCaravelaInDifferentNamespace()
        {
            this.TestOneLicense( TestLicenseKeys.OpenSource, LicensedFeatures.Caravela, "Foo", false );
        }
    }
}
