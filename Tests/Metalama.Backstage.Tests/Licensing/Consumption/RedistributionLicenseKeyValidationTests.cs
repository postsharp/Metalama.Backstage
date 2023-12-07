// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing.Consumption
{
    public class RedistributionLicenseKeyValidationTests : LicenseConsumptionManagerTestsBase
    {
        public RedistributionLicenseKeyValidationTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Theory]
        [TestLicensesInlineData(
            nameof(TestLicenseKeys.PostSharpUltimateOpenSourceRedistribution),
            TestLicenseKeys.PostSharpUltimateOpenSourceRedistributionNamespace )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution), TestLicenseKeys.MetalamaUltimateRedistributionNamespace )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateCommercialRedistribution), TestLicenseKeys.MetalamaUltimateRedistributionNamespace )]
        public void RedistributionLicenseAllowsLicensedNamespace( string licenseKey, string requiredNamespace )
        {
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey( licenseKey, requiredNamespace );

            Assert.True( actualIsValid );
        }

        [Theory]
        [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpUltimateOpenSourceRedistribution) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution) )]
        [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateCommercialRedistribution) )]
        public void RedistributionLicenseForbidsArbitraryNamespace( string licenseKey )
        {
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey( licenseKey, "Foo" );

            Assert.False( actualIsValid );
        }

        [Fact]
        public void NonRedistributableLicenseKeyFailsRedistributionLicenseKeyValidation()
        {
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey(
                TestLicenseKeys.MetalamaUltimatePersonalProjectBound,
                TestLicenseKeys.MetalamaUltimateProjectBoundProjectName );

            Assert.False( actualIsValid );
        }
    }
}