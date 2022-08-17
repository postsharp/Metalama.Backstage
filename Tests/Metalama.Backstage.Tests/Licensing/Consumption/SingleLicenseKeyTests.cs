// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Consumption;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Consumption;

public class SingleLicenseKeyTests : LicenseConsumptionManagerTestsBase
{
    public SingleLicenseKeyTests( ITestOutputHelper logger )
        : base( logger ) { }

    private void TestOneLicense( string licenseKey, LicensedFeatures requiredFeatures, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, requiredFeatures, "Foo", expectedCanConsume );

    private void TestOneLicense(
        string licenseKey,
        LicensedFeatures requiredFeatures,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var license = this.CreateLicense( licenseKey );
        var manager = this.CreateConsumptionManager( license );
        TestConsumption( manager, requiredFeatures, requiredNamespace, expectedCanConsume );
    }

    [Fact]
    public void NoLicenseAutoRegistersEvaluationLicense()
    {
        LicenseConsumptionManager manager = new( this.ServiceProvider );
        TestConsumption( manager, LicensedFeatures.MetalamaAspects, false, true );
    }

    [Fact]
    public void UltimateLicenseAllowsMetalamaFeature() => this.TestOneLicense( TestLicenseKeys.PostSharpUltimate, LicensedFeatures.MetalamaAspects, true );

    [Fact]
    public void LoggingLicenseForbidsMetalamaFeature() => this.TestOneLicense( TestLicenseKeys.Logging, LicensedFeatures.MetalamaAspects, false );

    [Fact]
    public void OpenSourceAllowsMetalamaInSameNamespace()
        => this.TestOneLicense(
            TestLicenseKeys.OpenSource,
            LicensedFeatures.MetalamaAspects,
            TestLicenseKeys.OpenSourceNamespace,
            true );

    [Fact]
    public void OpenSourceForbidsMetalamaInDifferentNamespace() => this.TestOneLicense( TestLicenseKeys.OpenSource, LicensedFeatures.MetalamaAspects, "Foo", false );

    [Fact]
    public void MetalamaLicenseAllowsMetalamaFeature() => this.TestOneLicense( TestLicenseKeys.MetalamaUltimate, LicensedFeatures.MetalamaAspects, true );

    [Fact]
    public void MetalamaLicenseAllowsEssentialsFeature() => this.TestOneLicense( TestLicenseKeys.MetalamaUltimate, LicensedFeatures.Essentials, true );

    [Fact]
    public void PostSharpEssentialsLicenseForbidsMetalamaFeature() => this.TestOneLicense( TestLicenseKeys.PostSharpEssentials, LicensedFeatures.MetalamaAspects, false );

    [Fact]
    public void PostSharpEssentialsLicenseAllowsEssentialsFeature() => this.TestOneLicense( TestLicenseKeys.PostSharpEssentials, LicensedFeatures.Essentials, true );

    [Fact]
    public void MetalamaEssentialsLicenseForbidsMetalamaFeature() => this.TestOneLicense( TestLicenseKeys.MetalamaUltimateEssentials, LicensedFeatures.MetalamaAspects, false );

    [Fact]
    public void MetalamaEssentialsLicenseAllowsEssentialsFeature() => this.TestOneLicense( TestLicenseKeys.MetalamaUltimateEssentials, LicensedFeatures.Essentials, true );

    [Fact]
    public void SelfSignedMetalamaEssentialsLicenseForbidsMetalamaFeature()
        => this.TestOneLicense( TestLicenseKeys.CreateMetalamaEssentialsLicense( this.ServiceProvider ), LicensedFeatures.MetalamaAspects, false );

    [Fact]
    public void SelfSignedMetalamaEssentialsLicenseAllowsEssentialsFeature()
        => this.TestOneLicense( TestLicenseKeys.CreateMetalamaEssentialsLicense( this.ServiceProvider ), LicensedFeatures.Essentials, true );
}