// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Tests.Licensing.LicenseSources;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Consumption;

public class LicenseUsageTests : LicenseConsumptionManagerTestsBase
{
    public LicenseUsageTests( ITestOutputHelper logger )
        : base( logger ) { }

    private static void AssertAllUsed( params IUsable[] usableObjects ) => Assert.Equal( usableObjects.Length, usableObjects.Count( l => l.IsUsed ) );

    [Fact]
    public void FirstOfDifferentLicensesFromMultipleSourcesUsedForAllowedFeature()
    {
        var license1 = this.CreateLicense( TestLicenses.MetalamaFreePersonal );
        var source1 = new TestLicenseSource( "source1", license1 );

        var license2 = this.CreateLicense( TestLicenses.MetalamaStarterBusiness );
        var source2 = new TestLicenseSource( "source2", license2 );

        var manager = this.CreateConsumptionManager( source1, source2 );
        TestConsumption( manager, LicenseRequirementTestEnum.Free, true );
        Assert.True( license1.IsUsed );
        Assert.False( license2.IsUsed );
        Assert.True( source1.IsUsed );
        Assert.False( source2.IsUsed );
    }

    [Fact]
    public void OneOfDifferentLicensesFromMultipleSourcesUsedForForbiddenFeature()
    {
        var license1 = this.CreateLicense( TestLicenses.MetalamaFreePersonal );
        var source1 = new TestLicenseSource( "source1", license1 );

        var license2 = this.CreateLicense( TestLicenses.MetalamaStarterBusiness );
        var source2 = new TestLicenseSource( "source2", license2 );

        var manager = this.CreateConsumptionManager( source1, source2 );
        TestConsumption( manager, LicenseRequirementTestEnum.Ultimate, false );
        Assert.True( license1.IsUsed );
        Assert.False( license2.IsUsed );
        Assert.True( source1.IsUsed );
        Assert.False( source2.IsUsed );
    }

    [Fact]
    public void NamespaceLicenseConsideredForForbiddenFeature()
    {
        var license = this.CreateLicense( TestLicenses.MetalamaUltimateOpenSourceRedistribution );
        var manager = this.CreateConsumptionManager( license );
        TestConsumption( manager, LicenseRequirementTestEnum.Ultimate, "Foo", false );
        AssertAllUsed( license );
    }
}