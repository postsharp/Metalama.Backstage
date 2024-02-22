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
        [InlineData(
            nameof(TestLicenseKeys.PostSharpUltimateOpenSourceRedistribution),
            TestLicenseKeys.PostSharpUltimateOpenSourceRedistributionNamespace )]
        [InlineData( nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution), TestLicenseKeys.MetalamaUltimateRedistributionNamespace )]
        [InlineData( nameof(TestLicenseKeys.MetalamaUltimateCommercialRedistribution), TestLicenseKeys.MetalamaUltimateRedistributionNamespace )]
        public void RedistributionLicenseAllowsLicensedNamespace( string licenseKeyName, string requiredNamespace )
        {
            var licenseKey = TestLicenseKeys.GetLicenseKey( licenseKeyName );
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey( licenseKey, requiredNamespace );

            Assert.True( actualIsValid );
        }

        [Theory]
        [InlineData( nameof(TestLicenseKeys.PostSharpUltimateOpenSourceRedistribution) )]
        [InlineData( nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution) )]
        [InlineData( nameof(TestLicenseKeys.MetalamaUltimateCommercialRedistribution) )]
        public void RedistributionLicenseForbidsArbitraryNamespace( string licenseKeyName )
        {
            var licenseKey = TestLicenseKeys.GetLicenseKey( licenseKeyName );
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