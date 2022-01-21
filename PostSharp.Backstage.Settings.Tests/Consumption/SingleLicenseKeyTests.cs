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
            this.TestOneLicense( licenseKey, requiredFeatures, "Foo", expectedCanConsume );
        }

        private void TestOneLicense(
            string licenseKey,
            LicensedFeatures requiredFeatures,
            string requiredNamespace,
            bool expectedCanConsume,
            bool expectedLicenseAutoRegistrationAttempt = false )
        {
            var license = this.CreateLicense( licenseKey );
            var manager = this.CreateConsumptionManager( license );
            this.TestConsumption( manager, requiredFeatures, requiredNamespace, expectedCanConsume );
            Assert.Equal( expectedLicenseAutoRegistrationAttempt, this.AutoRegistrar.RegistrationAttempted );
        }

        [Fact]
        public void NoLicenseAutoRegistersEvaluationLicense()
        {
            LicenseConsumptionManager manager = new(this.Services);
            this.TestConsumption( manager, LicensedFeatures.Metalama, false, true );
        }

        [Fact]
        public void UltimateLicenseAllowsMetalamaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Ultimate, LicensedFeatures.Metalama, true );
        }

        [Fact]
        public void LoggingLicenseForbidsMetalamaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Logging, LicensedFeatures.Metalama, false );
        }

        [Fact]
        public void OpenSourceAllowsMetalamaInSameNamespace()
        {
            this.TestOneLicense( 
                TestLicenseKeys.OpenSource,
                LicensedFeatures.Metalama,
                TestLicenseKeys.OpenSourceNamespace,
                true );
        }

        [Fact]
        public void OpenSourceForbidsMetalamaInDifferentNamespace()
        {
            this.TestOneLicense( TestLicenseKeys.OpenSource, LicensedFeatures.Metalama, "Foo", false );
        }

        [Fact]
        public void MetalamaLicenseAllowsMetalamaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Metalama, LicensedFeatures.Metalama, true );
        }

        [Fact]
        public void MetalamaLicenseAllowsCommunityFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Metalama, LicensedFeatures.Community, true );
        }

        [Fact]
        public void CommunityLicenseForbidsMetalamaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Community, LicensedFeatures.Metalama, false );
        }

        [Fact]
        public void CommunityLicenseAllowsCommunityFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Community, LicensedFeatures.Community, true );
        }
    }
}