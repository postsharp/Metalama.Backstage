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
        [InlineData( TestLicenses.PostSharpUltimateOpenSourceRedistribution, int.MaxValue )]
        [InlineData( TestLicenses.MetalamaStarterPersonal, 5 )]
        [InlineData( TestLicenses.MetalamaStarterBusiness, 5 )]
        [InlineData( TestLicenses.MetalamaProfessionalPersonal, 10 )]
        [InlineData( TestLicenses.MetalamaProfessionalBusiness, 10 )]
        [InlineData( TestLicenses.MetalamaUltimatePersonal, int.MaxValue )]
        [InlineData( TestLicenses.MetalamaUltimateBusiness, int.MaxValue )]
        [InlineData( TestLicenses.MetalamaFreePersonal, 3 )]
        [InlineData( TestLicenses.MetalamaFreeBusiness, 3 )]
        [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, int.MaxValue )]
        [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, int.MaxValue )]
        public void ExpectedMaxAspectsCountGiven( string licenseKey, int expectedMaxAspectsCount )
        {
            var license = this.CreateLicense( licenseKey );
            var manager = this.CreateConsumptionManager( license );
            var actualMaxAspectsCount = manager.MaxAspectsCount;

            Assert.Equal( expectedMaxAspectsCount, actualMaxAspectsCount );
        }
    }
}