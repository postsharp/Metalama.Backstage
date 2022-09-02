// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Tests.Licensing.Licenses;
using Metalama.Backstage.Licensing.Tests.Licensing.LicenseSources;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Consumption;

public abstract class LicenseConsumptionManagerTestsBase : LicensingTestsBase
{
    private protected LicenseConsumptionManagerTestsBase(
        ITestOutputHelper logger,
        Action<ServiceProviderBuilder>? serviceBuilder = null )
        : base( logger, serviceBuilder ) { }

    private protected TestLicense CreateLicense( string licenseString )
    {
        Assert.True( this.LicenseFactory.TryCreate( licenseString, out var license ) );

        return new TestLicense( license! );
    }

    private protected ILicenseConsumptionManager CreateConsumptionManager( params TestLicense[] licenses )
    {
        // ReSharper disable once CoVariantArrayConversion
        var licenseSource = new TestLicenseSource( "test", licenses );

        return this.CreateConsumptionManager( licenseSource );
    }

    private protected ILicenseConsumptionManager CreateConsumptionManager( params ILicenseSource[] licenseSources )
        => new LicenseConsumptionManager( this.ServiceProvider, licenseSources );

    private protected static void TestConsumption(
        ILicenseConsumptionManager manager,
        LicensedFeatures requiredFeatures,
        bool expectedCanConsume,
        bool expectedLicenseAutoRegistrationAttempt = false )
        => TestConsumption(
            manager,
            requiredFeatures,
            "Foo",
            expectedCanConsume );

    private protected static void TestConsumption(
        ILicenseConsumptionManager manager,
        LicensedFeatures requiredFeatures,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var actualCanConsume = manager.CanConsumeFeatures( requiredFeatures, requiredNamespace );
        Assert.Equal( expectedCanConsume, actualCanConsume );
    }
}