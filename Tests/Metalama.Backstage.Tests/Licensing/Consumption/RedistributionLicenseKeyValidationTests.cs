// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing.Consumption
{
    public class RedistributionLicenseKeyValidationTests : LicenseConsumptionManagerTestsBase
    {
        public RedistributionLicenseKeyValidationTests( ITestOutputHelper logger )
            : base( logger ) { }

        [Theory]
        [InlineData( TestLicenses.PostSharpUltimateOpenSourceRedistribution, TestLicenses.PostSharpUltimateOpenSourceRedistributionNamespace )]
        [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, TestLicenses.MetalamaUltimateRedistributionNamespace )]
        [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, TestLicenses.MetalamaUltimateRedistributionNamespace )]
        public void RedistributionLicenseAllowsLicensedNamespace( string licenseKey, string requiredNamespace )
        {
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey( licenseKey, requiredNamespace );

            Assert.True( actualIsValid );
        }

        [Theory]
        [InlineData( TestLicenses.PostSharpUltimateOpenSourceRedistribution )]
        [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution )]
        [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution )]
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
                TestLicenses.MetalamaUltimatePersonalProjectBound,
                TestLicenses.MetalamaUltimateProjectBoundProjectName );
            
            Assert.False( actualIsValid );
        }
    }
}