// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Tests.Licenses;
using PostSharp.Backstage.Licensing.Tests.Registration;
using PostSharp.Backstage.Testing.Services;
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
            this.Services.SetService<IFirstRunLicenseActivator>( this.AutoRegistrar );
        }

        private protected ILicenseConsumer CreateConsumer( string requiredNamespace = "Foo", string diagnosticsLocationDescription = "TestLocation" )
        {
            return new TestLicenseConsumer( requiredNamespace, targetTypeName: "Bar", diagnosticsLocationDescription, this.Trace );
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
            return new LicenseConsumptionManager( this.Services, licenseSources );
        }

        private protected void TestConsumption(
            ILicenseConsumptionManager manager,
            LicensedFeatures requiredFeatures,
            bool expectedCanConsume,
            bool expectedLicenseAutoRegistrationAttempt = false )
        {
            this.TestConsumption( manager, requiredFeatures, "Foo", expectedCanConsume, expectedLicenseAutoRegistrationAttempt );
        }

        private protected void TestConsumption(
            ILicenseConsumptionManager manager,
            LicensedFeatures requiredFeatures,
            string reuqiredNamespace,
            bool expectedCanConsume,
            bool expectedLicenseAutoRegistrationAttempt = false )
        {
            var consumer = this.CreateConsumer( reuqiredNamespace );

            void TestCanConsume()
            {
                var actualCanConsume = manager.CanConsumeFeature( consumer, requiredFeatures );
                this.Diagnostics.AssertClean();
                consumer.Diagnostics.AssertClean();
                Assert.Equal( expectedCanConsume, actualCanConsume );
            }

            void TestConsume()
            {
                manager.ConsumeFeature( consumer, requiredFeatures );

                this.Diagnostics.AssertClean();

                if (expectedCanConsume)
                {
                    consumer.Diagnostics.AssertClean();
                }
                else
                {
                    consumer.Diagnostics.AssertNoWarnings();
                    consumer.Diagnostics.AssertSingleError( "No license available for feature(s) Caravela required by 'Bar' type.", consumer.DiagnosticsLocation );
                }
            }

            TestCanConsume();
            TestConsume();

            Assert.Equal( expectedLicenseAutoRegistrationAttempt, this.AutoRegistrar.RegistrationAttempted );
        }
    }
}
