// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Tests.Licensing.Licenses;
using Metalama.Backstage.Tests.Licensing.LicenseSources;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing.Consumption;

public abstract class LicenseConsumptionManagerTestsBase : LicensingTestsBase
{
    private protected LicenseConsumptionManagerTestsBase(
        ITestOutputHelper logger,
        Action<ServiceProviderBuilder>? serviceBuilder = null )
        : base( logger, serviceBuilder ) { }

    private protected TestLicense CreateLicense( string licenseString )
    {
        Assert.True( this.LicenseFactory.TryCreate( licenseString, out var license, out var errorMessage ) );
        Assert.Null( errorMessage );

        return new TestLicense( license! );
    }

    private protected ILicenseConsumptionService CreateConsumptionManager( TestLicense license )
    {
        // ReSharper disable once CoVariantArrayConversion
        var licenseSource = new TestLicenseSource( "test", license );

        return this.CreateConsumptionManager( licenseSource );
    }

    private protected ILicenseConsumptionService CreateConsumptionManager( params ILicenseSource[] licenseSources )
        => new LicenseConsumptionService( this.ServiceProvider, licenseSources );

    private protected ILicenseConsumptionService CreateConsumptionManager()
        => new LicenseConsumptionService( this.ServiceProvider, Enumerable.Empty<ILicenseSource>() );

    private protected static void TestConsumption(
        ILicenseConsumptionService service,
        LicenseRequirementTestEnum requestedRequirement,
        bool expectedCanConsume )
        => TestConsumption(
            service,
            requestedRequirement,
            "Foo",
            expectedCanConsume );

    private protected static void TestConsumption(
        ILicenseConsumptionService service,
        LicenseRequirementTestEnum requestedRequirement,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var actualCanConsume = service.CanConsume( LicenseRequirementHelper.GetRequirement( requestedRequirement ), requiredNamespace );
        Assert.Equal( expectedCanConsume, actualCanConsume );
    }
}