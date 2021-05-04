// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
        }

        [Fact]
        public void NoLicenseAutoRegistersEvaluationLicense()
        {
            // TODO

            throw new System.NotImplementedException();
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
