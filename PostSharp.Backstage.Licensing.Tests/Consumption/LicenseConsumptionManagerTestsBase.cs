using System;
using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Consumption.Sources;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Tests.Licenses;
using PostSharp.Backstage.Licensing.Tests.LicenseSources;
using PostSharp.Backstage.Licensing.Tests.Registration;
using PostSharp.Backstage.Testing.Services;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Licensing.Tests.Consumption
{
    public abstract class LicenseConsumptionManagerTestsBase : LicensingTestsBase
    {
        private protected TestFirstRunLicenseActivator AutoRegistrar { get; }

        private protected LicenseConsumptionManagerTestsBase(
            ITestOutputHelper logger,
            Action<BackstageServiceCollection>? serviceBuilder = null )
            : base(
                logger,
                serviceCollection =>
                {
                    serviceCollection
                        .AddSingleton<IFirstRunLicenseActivator>( new TestFirstRunLicenseActivator() );

                    serviceBuilder?.Invoke( serviceCollection );
                } )
        {
            AutoRegistrar = (TestFirstRunLicenseActivator)Services.GetRequiredService<IFirstRunLicenseActivator>();
        }

        private protected ILicenseConsumer CreateConsumer(
            string requiredNamespace = "Foo",
            string diagnosticsLocationDescription = "TestLocation" )
        {
            return new TestLicenseConsumer(
                requiredNamespace,
                "Bar",
                diagnosticsLocationDescription,
                Services );
        }

        private protected TestLicense CreateLicense( string licenseString )
        {
            Assert.True( LicenseFactory.TryCreate( licenseString, out var license ) );

            return new TestLicense( license! );
        }

        private protected ILicenseConsumptionManager CreateConsumptionManager( params TestLicense[] licenses )
        {
            // ReSharper disable once CoVariantArrayConversion
            var licenseSource = new TestLicenseSource( "test", licenses );

            return CreateConsumptionManager( licenseSource );
        }

        private protected ILicenseConsumptionManager CreateConsumptionManager( params ILicenseSource[] licenseSources )
        {
            return new LicenseConsumptionManager( Services, licenseSources );
        }

        private protected void TestConsumption(
            ILicenseConsumptionManager manager,
            LicensedFeatures requiredFeatures,
            bool expectedCanConsume,
            bool expectedLicenseAutoRegistrationAttempt = false )
        {
            TestConsumption(
                manager,
                requiredFeatures,
                "Foo",
                expectedCanConsume,
                expectedLicenseAutoRegistrationAttempt );
        }

        private protected void TestConsumption(
            ILicenseConsumptionManager manager,
            LicensedFeatures requiredFeatures,
            string requiredNamespace,
            bool expectedCanConsume,
            bool expectedLicenseAutoRegistrationAttempt = false )
        {
            var consumer = CreateConsumer( requiredNamespace );

            void TestCanConsume()
            {
                var actualCanConsume = manager.CanConsumeFeatures( consumer, requiredFeatures );
                Diagnostics.AssertClean();
                consumer.Diagnostics.AssertClean();
                Assert.Equal( expectedCanConsume, actualCanConsume );
            }

            void TestConsume()
            {
                manager.ConsumeFeatures( consumer, requiredFeatures );

                Diagnostics.AssertClean();

                if (expectedCanConsume)
                {
                    consumer.Diagnostics.AssertClean();
                }
                else
                {
                    consumer.Diagnostics.AssertNoWarnings();

                    consumer.Diagnostics.AssertSingleError(
                        "No license available for feature(s) Caravela required by 'Bar' type.",
                        consumer.DiagnosticsLocation );
                }
            }

            TestCanConsume();
            TestConsume();

            Assert.Equal( expectedLicenseAutoRegistrationAttempt, AutoRegistrar.RegistrationAttempted );
        }
    }
}