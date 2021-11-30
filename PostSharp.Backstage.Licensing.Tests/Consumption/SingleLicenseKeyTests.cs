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
            : base( logger ) { }

        private void TestOneLicense( string licenseKey, LicensedFeatures requiredFeatures, bool expectedCanConsume )
        {
            TestOneLicense( licenseKey, requiredFeatures, "Foo", expectedCanConsume );
        }

        private void TestOneLicense(
            string licenseKey,
            LicensedFeatures requiredFeatures,
            string requiredNamespace,
            bool expectedCanConsume,
            bool expectedLicenseAutoRegistrationAttempt = false )
        {
            var license = CreateLicense( licenseKey );
            var manager = CreateConsumptionManager( license );
            TestConsumption( manager, requiredFeatures, requiredNamespace, expectedCanConsume );
            Assert.Equal( expectedLicenseAutoRegistrationAttempt, AutoRegistrar.RegistrationAttempted );
        }

        [Fact]
        public void NoLicenseAutoRegistersEvaluationLicense()
        {
            LicenseConsumptionManager manager = new( Services );
            TestConsumption( manager, LicensedFeatures.Caravela, false, true );
        }

        [Fact]
        public void UltimateLicenseAllowsCaravelaFeature()
        {
            TestOneLicense( TestLicenseKeys.Ultimate, LicensedFeatures.Caravela, true );
        }

        [Fact]
        public void LoggingLicenseForbidsCaravelaFeature()
        {
            TestOneLicense( TestLicenseKeys.Logging, LicensedFeatures.Caravela, false );
        }

        [Fact]
        public void OpenSourceAllowsCaravelaInSameNamespace()
        {
            TestOneLicense( TestLicenseKeys.OpenSource, LicensedFeatures.Caravela, TestLicenseKeys.OpenSourceNamespace, true );
        }

        [Fact]
        public void OpenSourceForbidsCaravelaInDifferentNamespace()
        {
            TestOneLicense( TestLicenseKeys.OpenSource, LicensedFeatures.Caravela, "Foo", false );
        }

        [Fact]
        public void CaravelaLicenseAllowsCaravelaFeature()
        {
            TestOneLicense( TestLicenseKeys.Caravela, LicensedFeatures.Caravela, true );
        }

        [Fact]
        public void CaravelaLicenseAllowsCommunityFeature()
        {
            TestOneLicense( TestLicenseKeys.Caravela, LicensedFeatures.Community, true );
        }

        [Fact]
        public void CommunityLicenseForbidsCaravelaFeature()
        {
            TestOneLicense( TestLicenseKeys.Community, LicensedFeatures.Caravela, false );
        }

        [Fact]
        public void CommunityLicenseAllowsCommunityFeature()
        {
            TestOneLicense( TestLicenseKeys.Community, LicensedFeatures.Community, true );
        }
    }
}