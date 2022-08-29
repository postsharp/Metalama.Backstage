// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Consumption
{
    public class RedistributionLicenseKeyValidationTests : LicenseConsumptionManagerTestsBase
    {
        public RedistributionLicenseKeyValidationTests( ITestOutputHelper logger )
            : base( logger )
        {
        }

        [Theory]
        [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution )]
        [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution )]
        public void NamespaceUnlimitedRedistributionLicenseAllowsArbitraryNamespace( string licenseKey )
        {
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey( licenseKey, "Foo" );

            Assert.True( actualIsValid );
        }

        [Theory]
        [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, TestLicenses.NamespaceLimitedPostSharpUltimateOpenSourceNamespace )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, TestLicenses.NamespaceLimitedMetalamaUltimateRedistributionNamespace )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution, TestLicenses.NamespaceLimitedMetalamaUltimateRedistributionNamespace )]
        public void NamespaceLimitedRedistributionLicenseAllowsLicensedNamespace( string licenseKey, string requiredNamespace )
        {
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey( licenseKey, requiredNamespace );

            Assert.True( actualIsValid );
        }

        [Theory]
        [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution )]
        public void NamespaceLimitedRedistributionLicenseForbidsArbitraryNamespace( string licenseKey )
        {
            var manager = this.CreateConsumptionManager();

            var actualIsValid = manager.ValidateRedistributionLicenseKey( licenseKey, "Foo" );

            Assert.False( actualIsValid );
        }
    }
}