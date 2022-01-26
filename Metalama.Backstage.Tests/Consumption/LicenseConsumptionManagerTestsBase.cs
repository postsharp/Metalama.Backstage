// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Licensing.Tests.Licenses;
using Metalama.Backstage.Licensing.Tests.LicenseSources;
using Metalama.Backstage.Licensing.Tests.Registration;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Consumption;

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
        => new LicenseConsumptionManager( this.Services, licenseSources );

    private protected void TestConsumption(
        ILicenseConsumptionManager manager,
        LicensedFeatures requiredFeatures,
        bool expectedCanConsume,
        bool expectedLicenseAutoRegistrationAttempt = false )
        => this.TestConsumption(
            manager,
            requiredFeatures,
            "Foo",
            expectedCanConsume );

    private protected void TestConsumption(
        ILicenseConsumptionManager manager,
        LicensedFeatures requiredFeatures,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var actualCanConsume = manager.CanConsumeFeatures( requiredFeatures, requiredNamespace );
        Assert.Equal( expectedCanConsume, actualCanConsume );
    }
}