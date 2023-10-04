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
            nameof(TestLicenses.PostSharpUltimateOpenSourceRedistribution),
            TestLicenses.PostSharpUltimateOpenSourceRedistributionNamespace )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateOpenSourceRedistribution), TestLicenses.MetalamaUltimateRedistributionNamespace )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateCommercialRedistribution), TestLicenses.MetalamaUltimateRedistributionNamespace )]
        public void RedistributionLicenseAllowsLicensedNamespace( string licenseKey, string requiredNamespace )
        {
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey( licenseKey, requiredNamespace );

            Assert.True( actualIsValid );
        }

        [Theory]
        [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimateOpenSourceRedistribution) )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateOpenSourceRedistribution) )]
        [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateCommercialRedistribution) )]
        public void RedistributionLicenseForbidsArbitraryNamespace( string licenseKey )
        {
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey( licenseKey, "Foo" );

            Assert.False( actualIsValid );
        }
    }
}