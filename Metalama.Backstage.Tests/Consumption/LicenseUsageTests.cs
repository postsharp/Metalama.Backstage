// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Tests.LicenseSources;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Consumption
{
    public class LicenseUsageTests : LicenseConsumptionManagerTestsBase
    {
        public LicenseUsageTests( ITestOutputHelper logger )
            : base( logger ) { }

        private void AssertOneUsed( bool expectAutoRegistration, params IUsable[] usableObjects )
        {
            Assert.Equal( 1, usableObjects.Count( l => l.IsUsed ) );
            Assert.Equal( expectAutoRegistration, this.AutoRegistrar.RegistrationAttempted );
        }

        private void AssertAllUsed( bool expectAutoRegistration, params IUsable[] usableObjects )
        {
            Assert.Equal( usableObjects.Length, usableObjects.Count( l => l.IsUsed ) );
            Assert.Equal( expectAutoRegistration, this.AutoRegistrar.RegistrationAttempted );
        }

        [Fact]
        public void OneOfEqualLicensesFromOneSourceUsedForAllowedFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var license2 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Metalama, true );
            this.AssertOneUsed( false, license1, license2 );
        }

        [Fact]
        public void OneEqualLicenseFromOneSourceUsedForForbiddenFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Logging );
            var license2 = this.CreateLicense( TestLicenseKeys.Logging );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Metalama, false );
            this.AssertOneUsed( false, license1, license2 );
        }

        [Fact]
        public void AllDifferentLicensesFromOneSourceUsedForForbiddenFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Logging );
            var license2 = this.CreateLicense( TestLicenseKeys.Caching );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Metalama, false );
            this.AssertAllUsed( false, license1, license2 );
        }

        [Fact]
        public void OneOfEqualLicensesFromMultipleSourcesUsedForAllowedFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var source1 = new TestLicenseSource( "source1", license1 );

            var license2 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var source2 = new TestLicenseSource( "source2", license2 );

            var manager = this.CreateConsumptionManager( source1, source2 );
            this.TestConsumption( manager, LicensedFeatures.Metalama, true );
            this.AssertOneUsed( false, license1, license2 );
            this.AssertOneUsed( false, source1, source2 );
        }

        [Fact]
        public void AllOfDifferentLicensesFromMultipleSourcesUsedForForbiddenFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Logging );
            var source1 = new TestLicenseSource( "source1", license1 );

            var license2 = this.CreateLicense( TestLicenseKeys.Caching );
            var source2 = new TestLicenseSource( "source2", license2 );

            var manager = this.CreateConsumptionManager( source1, source2 );
            this.TestConsumption( manager, LicensedFeatures.Metalama, false );
            this.AssertAllUsed( false, license1, license2 );
            this.AssertAllUsed( false, source1, source2 );
        }

        [Fact]
        public void NamespaceLicenseNotPreferredForAllowedFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var license2 = this.CreateLicense( TestLicenseKeys.OpenSource );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Metalama, TestLicenseKeys.OpenSourceNamespace, true );
            Assert.True( license1.IsUsed );
            Assert.False( license2.IsUsed );
        }

        [Fact]
        public void NamespaceLicenseConsideredForForbiddenFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Logging );
            var license2 = this.CreateLicense( TestLicenseKeys.OpenSource );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Metalama, "Foo", false );
            this.AssertAllUsed( false, license1, license2 );
        }
    }
}