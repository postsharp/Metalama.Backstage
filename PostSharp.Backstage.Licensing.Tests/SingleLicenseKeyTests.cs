// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Tests.Consumption;
using PostSharp.Backstage.Licensing.Tests.Services;
using System;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests
{
    public class SingleLicenseKeyTests
    {
        private readonly ITrace _trace;

        public SingleLicenseKeyTests( ITestOutputHelper logger )
        {
            this._trace = new TestTrace( logger );
        }

        private static ILicenseConsumer CreateConsumer()
        {
            return new TestLicenseConsumer( "Foo", "Bar", null );
        }

        private ILicenseConsumptionManager CreateConsumptionManager( IServiceProvider services, string licenseString )
        {
            var licenseFactory = new LicenseFactory( services, this._trace );
            Assert.True( licenseFactory.TryCreate( licenseString, out var license ) );
            var licenseSource = new TestLicenseSource( "test", license! );
            return new LicenseConsumptionManager( services, this._trace, licenseSource );
        }

        private void TestOneLicense( string licenseKey, LicensedFeatures requiredFeatures, bool expectedCanConsume )
        {
            var services = new TestServices();
            var consumer = CreateConsumer();
            var manager = CreateConsumptionManager( services, licenseKey );
            var actualCanConsume = manager.CanConsumeFeature( consumer, requiredFeatures );
            services.Diagnostics.AssertClean();
            consumer.Diagnostics.AssertClean();
            Assert.Equal( expectedCanConsume, actualCanConsume );
        }

        [Fact]
        public void TestUltimate()
        {
            this.TestOneLicense( TestLicenseKeys.Ultimate, LicensedFeatures.Caravela, true );
        }

        [Fact]
        public void TestLogging()
        {
            this.TestOneLicense( TestLicenseKeys.Logging, LicensedFeatures.Caravela, false );
        }
    }
}
