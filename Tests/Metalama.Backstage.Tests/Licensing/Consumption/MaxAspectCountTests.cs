// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Consumption
{
    public class MaxAspectCountTests : LicenseConsumptionManagerTestsBase
    {
        public MaxAspectCountTests( ITestOutputHelper logger )
            : base( logger )
        {
        }

        [Theory]
        [InlineData( TestLicenses.PostSharpEssentials, 3 )]
        [InlineData( TestLicenses.PostSharpFramework, 10 )]
        [InlineData( TestLicenses.PostSharpUltimate, int.MaxValue )]
        [InlineData( TestLicenses.PostSharpEnterprise, int.MaxValue )]
        [InlineData( TestLicenses.Caching, 0 )]
        [InlineData( TestLicenses.Logging, 0 )]
        [InlineData( TestLicenses.Model, 0 )]
        [InlineData( TestLicenses.Threading, 0 )]
        [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, 0 )]
        [InlineData( TestLicenses.MetalamaStarterBusiness, 5 )]
        [InlineData( TestLicenses.MetalamaProfessionalBusiness, 10 )]
        [InlineData( TestLicenses.MetalamaUltimateBusiness, int.MaxValue )]
        [InlineData( TestLicenses.MetalamaUltimateEssentials, 3 )]
        [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, int.MaxValue )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, 0 )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials, 0 )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, 0 )]
        public void ExpectedMaxAspectsCountGivenForArbitraryNamespace( string licenseKey, int expectedMaxAspectsCount )
        {
            var license = this.CreateLicense( licenseKey );
            var manager = this.CreateConsumptionManager( license );
            var actualMaxAspectsCount = manager.GetNamespaceUnlimitedMaxAspectsCount();

            Assert.Equal( expectedMaxAspectsCount, actualMaxAspectsCount );
        }

        [Theory]
        [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, TestLicenses.NamespaceLimitedPostSharpUltimateOpenSourceNamespace, int.MaxValue )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, TestLicenses.NamespaceLimitedMetalamaUltimateBusinessNamespace, int.MaxValue )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials, TestLicenses.NamespaceLimitedMetalamaUltimateEssentialsNamespace, 3 )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistributionNamespace, int.MaxValue )]

        public void ExpectedMaxAspectsCountGivenForLicensedNamespace( string licenseKey, string requiredNamespace, int expectedMaxAspectsCount )
        {
            var license = this.CreateLicense( licenseKey );
            var manager = this.CreateConsumptionManager( license );
            var actualMaxAspectsCounts = manager.GetNamespaceLimitedMaxAspectCounts();

            Assert.Single( actualMaxAspectsCounts, (requiredNamespace, expectedMaxAspectsCount) );

            var actualResult = manager.TryGetNamespaceLimitedMaxAspectsCount( requiredNamespace, out var actualMaxAspectsCount, out var actualLicensedNamespace );

            Assert.True( actualResult );
            Assert.Equal( expectedMaxAspectsCount, actualMaxAspectsCount );
            Assert.Equal( requiredNamespace, actualLicensedNamespace );
        }

        [Theory]
        [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials )]
        [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution )]
        public void NamespaceLimitedLicenseAllowsZeroAspectsNonLicensedNamespace( string licenseKey )
        {
            var license = this.CreateLicense( licenseKey );
            var manager = this.CreateConsumptionManager( license );

            var actualResult = manager.TryGetNamespaceLimitedMaxAspectsCount( "Foo", out var actualMaxAspectsCount, out var actualLicensedNamespace );

            Assert.False( actualResult );
            Assert.Equal( 0, actualMaxAspectsCount );
            Assert.Null( actualLicensedNamespace );
        }
    }
}