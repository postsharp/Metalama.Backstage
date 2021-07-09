﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Consumption
{
    public class LicenseUsageTests : LicenseConsumptionManagerTestsBase
    {
        public LicenseUsageTests( ITestOutputHelper logger )
            : base( logger ) { }

        private void AssertOneUsed( bool expectAutoRegistration, params IUsable[] usables )
        {
            Assert.Equal( 1, usables.Count( l => l.IsUsed ) );
            Assert.Equal( expectAutoRegistration, this.AutoRegistrar.RegistrationAttempted );
        }

        private void AssertAllUsed( bool expectAutoRegistration, params IUsable[] usables )
        {
            Assert.Equal( usables.Length, usables.Count( l => l.IsUsed ) );
            Assert.Equal( expectAutoRegistration, this.AutoRegistrar.RegistrationAttempted );
        }

        [Fact]
        public void OneOfEqualLicensesFromOneSourceUsedForAllowedFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var license2 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Caravela, true );
            this.AssertOneUsed( expectAutoRegistration: false, license1, license2 );
        }

        [Fact]
        public void OneEqualLicenseFromOneSourceUsedForForbiddenFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Logging );
            var license2 = this.CreateLicense( TestLicenseKeys.Logging );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Caravela, false );
            this.AssertOneUsed( expectAutoRegistration: false, license1, license2 );
        }

        [Fact]
        public void AllDifferentLicensesFromOneSourceUsedForForbiddenFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Logging );
            var license2 = this.CreateLicense( TestLicenseKeys.Caching );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Caravela, false );
            this.AssertAllUsed( expectAutoRegistration: false, license1, license2 );
        }

        [Fact]
        public void OneOfEqualLicensesFromMultipleSourcesUsedForAllowedFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var source1 = new TestLicenseSource( "source1", license1 );

            var license2 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var source2 = new TestLicenseSource( "source2", license2 );

            var manager = this.CreateConsumptionManager( source1, source2 );
            this.TestConsumption( manager, LicensedFeatures.Caravela, true );
            this.AssertOneUsed( expectAutoRegistration: false, license1, license2 );
            this.AssertOneUsed( expectAutoRegistration: false, source1, source2 );
        }

        [Fact]
        public void AllOfDifferentLicensesFromMultipleSourcesUsedForForbiddenFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Logging );
            var source1 = new TestLicenseSource( "source1", license1 );

            var license2 = this.CreateLicense( TestLicenseKeys.Caching );
            var source2 = new TestLicenseSource( "source2", license2 );

            var manager = this.CreateConsumptionManager( source1, source2 );
            this.TestConsumption( manager, LicensedFeatures.Caravela, false );
            this.AssertAllUsed( expectAutoRegistration: false, license1, license2 );
            this.AssertAllUsed( expectAutoRegistration: false, source1, source2 );
        }

        [Fact]
        public void NamspaceLicenseNotPreferedForAllowedFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Ultimate );
            var license2 = this.CreateLicense( TestLicenseKeys.OpenSource );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Caravela, TestLicenseKeys.OpenSourceNamespace, true );
            Assert.True( license1.IsUsed );
            Assert.False( license2.IsUsed );
        }

        [Fact]
        public void NamespaceLicenseConsideredForForbiddenFeature()
        {
            var license1 = this.CreateLicense( TestLicenseKeys.Logging );
            var license2 = this.CreateLicense( TestLicenseKeys.OpenSource );
            var manager = this.CreateConsumptionManager( license1, license2 );
            this.TestConsumption( manager, LicensedFeatures.Caravela, "Foo", false );
            this.AssertAllUsed( expectAutoRegistration: false, license1, license2 );
        }
    }
}