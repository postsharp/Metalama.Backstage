﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
            LicenseConsumptionManager manager = new( this.Services );
            this.TestConsumption( manager, LicensedFeatures.Caravela, false, true );
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

        [Fact]
        public void CaravelaLicenseAllowsCaravelaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Caravela, LicensedFeatures.Caravela, true );
        }

        [Fact]
        public void CaravelaLicenseAllowsCommunityFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Caravela, LicensedFeatures.Community, true );
        }

        [Fact]
        public void CommunityLicenseForbidsCaravelaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Community, LicensedFeatures.Caravela, false );
        }

        [Fact]
        public void CommunityLicenseAllowsCommunityFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Community, LicensedFeatures.Community, true );
        }
    }
}