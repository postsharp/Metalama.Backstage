// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Tests.LicenseSources;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Consumption
{
    public class LicenseUsageTests : LicenseConsumptionManagerTestsBase
    {
        public LicenseUsageTests( ITestOutputHelper logger )
            : base( logger ) { }

        private void AssertOneUsed( bool expectAutoRegistration, params IUsable[] usableObjects )
        {
            Assert.Equal( 1, usableObjects.Count( l => l.IsUsed ) );
            Assert.Equal( expectAutoRegistration, AutoRegistrar.RegistrationAttempted );
        }

        private void AssertAllUsed( bool expectAutoRegistration, params IUsable[] usableObjects )
        {
            Assert.Equal( usableObjects.Length, usableObjects.Count( l => l.IsUsed ) );
            Assert.Equal( expectAutoRegistration, AutoRegistrar.RegistrationAttempted );
        }

        [Fact]
        public void OneOfEqualLicensesFromOneSourceUsedForAllowedFeature()
        {
            var license1 = CreateLicense( TestLicenseKeys.Ultimate );
            var license2 = CreateLicense( TestLicenseKeys.Ultimate );
            var manager = CreateConsumptionManager( license1, license2 );
            TestConsumption( manager, LicensedFeatures.Caravela, true );
            AssertOneUsed( false, license1, license2 );
        }

        [Fact]
        public void OneEqualLicenseFromOneSourceUsedForForbiddenFeature()
        {
            var license1 = CreateLicense( TestLicenseKeys.Logging );
            var license2 = CreateLicense( TestLicenseKeys.Logging );
            var manager = CreateConsumptionManager( license1, license2 );
            TestConsumption( manager, LicensedFeatures.Caravela, false );
            AssertOneUsed( false, license1, license2 );
        }

        [Fact]
        public void AllDifferentLicensesFromOneSourceUsedForForbiddenFeature()
        {
            var license1 = CreateLicense( TestLicenseKeys.Logging );
            var license2 = CreateLicense( TestLicenseKeys.Caching );
            var manager = CreateConsumptionManager( license1, license2 );
            TestConsumption( manager, LicensedFeatures.Caravela, false );
            AssertAllUsed( false, license1, license2 );
        }

        [Fact]
        public void OneOfEqualLicensesFromMultipleSourcesUsedForAllowedFeature()
        {
            var license1 = CreateLicense( TestLicenseKeys.Ultimate );
            var source1 = new TestLicenseSource( "source1", license1 );

            var license2 = CreateLicense( TestLicenseKeys.Ultimate );
            var source2 = new TestLicenseSource( "source2", license2 );

            var manager = CreateConsumptionManager( source1, source2 );
            TestConsumption( manager, LicensedFeatures.Caravela, true );
            AssertOneUsed( false, license1, license2 );
            AssertOneUsed( false, source1, source2 );
        }

        [Fact]
        public void AllOfDifferentLicensesFromMultipleSourcesUsedForForbiddenFeature()
        {
            var license1 = CreateLicense( TestLicenseKeys.Logging );
            var source1 = new TestLicenseSource( "source1", license1 );

            var license2 = CreateLicense( TestLicenseKeys.Caching );
            var source2 = new TestLicenseSource( "source2", license2 );

            var manager = CreateConsumptionManager( source1, source2 );
            TestConsumption( manager, LicensedFeatures.Caravela, false );
            AssertAllUsed( false, license1, license2 );
            AssertAllUsed( false, source1, source2 );
        }

        [Fact]
        public void NamespaceLicenseNotPreferredForAllowedFeature()
        {
            var license1 = CreateLicense( TestLicenseKeys.Ultimate );
            var license2 = CreateLicense( TestLicenseKeys.OpenSource );
            var manager = CreateConsumptionManager( license1, license2 );
            TestConsumption( manager, LicensedFeatures.Caravela, TestLicenseKeys.OpenSourceNamespace, true );
            Assert.True( license1.IsUsed );
            Assert.False( license2.IsUsed );
        }

        [Fact]
        public void NamespaceLicenseConsideredForForbiddenFeature()
        {
            var license1 = CreateLicense( TestLicenseKeys.Logging );
            var license2 = CreateLicense( TestLicenseKeys.OpenSource );
            var manager = CreateConsumptionManager( license1, license2 );
            TestConsumption( manager, LicensedFeatures.Caravela, "Foo", false );
            AssertAllUsed( false, license1, license2 );
        }
    }
}