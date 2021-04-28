// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Licenses;
using PostSharp.Backstage.Licensing.Sources;
using PostSharp.Backstage.Licensing.Tests.Licenses;
using PostSharp.Backstage.Licensing.Tests.Services;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Consumption
{
    public abstract class LicenseConsumptionManagerTestsBase : LicensingTestsBase
    {
        private readonly LicenseFactory _licenseFactory;

        public LicenseConsumptionManagerTestsBase( ITestOutputHelper logger )
            : base( logger )
        {
            this._licenseFactory = new( this.Services, this.Trace );
        }

        protected static ILicenseConsumer CreateConsumer( string requiredNamespace = "Foo" )
        {
            // TODO: IDiagnosticsLocation

            return new TestLicenseConsumer( requiredNamespace, targetTypeName: "Bar", diagnosticsLocation: null );
        }

        private protected TestLicense CreateLicense( string licenseString )
        {
            Assert.True( this._licenseFactory.TryCreate( licenseString, out var license ) );
            return new TestLicense( license! );
        }

        private protected ILicenseConsumptionManager CreateConsumptionManager( params TestLicense[] licenses )
        {
            var licenseSource = new TestLicenseSource( "test", licenses );
            return this.CreateConsumptionManager( licenseSource );
        }

        private protected ILicenseConsumptionManager CreateConsumptionManager( params ILicenseSource[] licenseSources )
        {
            return new LicenseConsumptionManager( this.Services, this.Trace, licenseSources );
        }

        private protected void TestConsumption( ILicenseConsumptionManager manager, LicensedFeatures requiredFeatures, bool expectedCanConsume )
        {
            this.TestConsumption( manager, requiredFeatures, "Foo", expectedCanConsume );
        }

        private protected void TestConsumption( ILicenseConsumptionManager manager, LicensedFeatures requiredFeatures, string reuqiredNamespace, bool expectedCanConsume )
        {
            var consumer = CreateConsumer( reuqiredNamespace );
            var actualCanConsume = manager.CanConsumeFeature( consumer, requiredFeatures );
            this.Services.Diagnostics.AssertClean();
            consumer.Diagnostics.AssertClean();
            Assert.Equal( expectedCanConsume, actualCanConsume );
        }
    }
}
