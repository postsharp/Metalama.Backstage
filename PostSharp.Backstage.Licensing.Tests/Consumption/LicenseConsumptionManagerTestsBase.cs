// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Sources;
using PostSharp.Backstage.Licensing.Tests.Licenses;
using PostSharp.Backstage.Licensing.Tests.Registration;
using PostSharp.Backstage.Licensing.Tests.Services;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Consumption
{
    public abstract class LicenseConsumptionManagerTestsBase : LicensingTestsBase
    {
        private protected TestLicenseAutoRegistrar AutoRegistrar { get; } = new();

        public LicenseConsumptionManagerTestsBase( ITestOutputHelper logger )
            : base( logger )
        {
            this.Services.SetService<ILicenseAutoRegistrar>( this.AutoRegistrar );
        }

        private protected ILicenseConsumer CreateConsumer( string requiredNamespace = "Foo" )
        {
            // TODO: IDiagnosticsLocation

            return new TestLicenseConsumer( requiredNamespace, targetTypeName: "Bar", diagnosticsLocation: null, this.Trace );
        }

        private protected TestLicense CreateLicense( string licenseString )
        {
            Assert.True( this.LicenseFactory.TryCreate( licenseString, out var license ) );
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
            var consumer = this.CreateConsumer( reuqiredNamespace );
            var actualCanConsume = manager.CanConsumeFeature( consumer, requiredFeatures );

            if ( actualCanConsume && this.AutoRegistrar.RegistrationAttempted )
            {
                actualCanConsume = false;
            }

            this.Services.Diagnostics.AssertClean();
            consumer.Diagnostics.AssertClean();
            Assert.Equal( expectedCanConsume, actualCanConsume );
        }
    }
}
