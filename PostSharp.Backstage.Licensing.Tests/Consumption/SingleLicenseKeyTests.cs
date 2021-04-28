// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Tests.Services;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Consumption
{
    public class SingleLicenseKeyTests
    {
        private readonly ITrace _trace;

        public SingleLicenseKeyTests( ITestOutputHelper logger )
        {
            this._trace = new TestTrace( logger );
        }

        private static ILicenseConsumer CreateConsumer( string requiredNamespace )
        {
            // TODO: IDiagnosticsLocation

            return new TestLicenseConsumer( requiredNamespace, targetTypeName: "Bar", diagnosticsLocation: null );
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
            this.TestOneLicense( licenseKey, requiredFeatures, reuqiredNamespace: "Foo", expectedCanConsume );
        }

        private void TestOneLicense( string licenseKey, LicensedFeatures requiredFeatures, string reuqiredNamespace, bool expectedCanConsume )
        {
            var services = new TestServices();
            var consumer = CreateConsumer( reuqiredNamespace );
            var manager = this.CreateConsumptionManager( services, licenseKey );
            var actualCanConsume = manager.CanConsumeFeature( consumer, requiredFeatures );
            services.Diagnostics.AssertClean();
            consumer.Diagnostics.AssertClean();
            Assert.Equal( expectedCanConsume, actualCanConsume );
        }

        [Fact]
        public void UltimateLicenseAllowsCaravelaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Ultimate, LicensedFeatures.Caravela, true );
        }

        [Fact]
        public void LoggingLicenseForbidsCaravelaFeature()
        {
            this.TestOneLicense( TestLicenseKeys.Logging, LicensedFeatures.Caravela, false );
        }

        [Fact]
        public void OpenSourceAllowsCaravelaInSameNamespace()
        {
            this.TestOneLicense( TestLicenseKeys.OpenSource, LicensedFeatures.Caravela, TestLicenseKeys.OpenSourceNamespace, true );
        }

        [Fact]
        public void OpenSourceForbidsCaravelaInDifferentNamespace()
        {
            this.TestOneLicense( TestLicenseKeys.OpenSource, LicensedFeatures.Caravela, "Foo", false );
        }
    }
}
