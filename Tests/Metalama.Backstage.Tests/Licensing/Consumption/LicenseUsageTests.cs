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
    public void AllDifferentLicensesFromOneSourceUsedForForbiddenFeature()
    {
        var license1 = this.CreateLicense( TestLicenseKeys.Logging );
        var license2 = this.CreateLicense( TestLicenseKeys.Caching );
        var manager = this.CreateConsumptionManager( license1, license2 );
        TestConsumption( manager, LicensedFeatures.MetalamaAspects, false );
        AssertAllUsed( license1, license2 );
    }

    [Fact]
    public void AllOfDifferentLicensesFromMultipleSourcesUsedForForbiddenFeature()
    {
        var license1 = this.CreateLicense( TestLicenseKeys.Logging );
        var source1 = new TestLicenseSource( "source1", false, license1 );

        var license2 = this.CreateLicense( TestLicenseKeys.Caching );
        var source2 = new TestLicenseSource( "source2", false, license2 );

        var manager = this.CreateConsumptionManager( source1, source2 );
        TestConsumption( manager, LicensedFeatures.MetalamaAspects, false );
        AssertAllUsed( license1, license2 );
        AssertAllUsed( source1, source2 );
    }

    [Fact]
    public void NamespaceLicenseConsideredForForbiddenFeature()
    {
        var license1 = this.CreateLicense( TestLicenseKeys.Logging );
        var license2 = this.CreateLicense( TestLicenseKeys.OpenSource );
        var manager = this.CreateConsumptionManager( license1, license2 );
        TestConsumption( manager, LicensedFeatures.MetalamaAspects, "Foo", false );
        AssertAllUsed( license1, license2 );
    }
}