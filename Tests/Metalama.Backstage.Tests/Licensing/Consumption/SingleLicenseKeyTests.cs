// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
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

    private void TestOneLicenseSource( ILicenseSource licenseSource, LicensedFeatures requiredFeatures, bool expectedCanConsume )
        => this.TestOneLicenseSource( licenseSource, requiredFeatures, "Foo", expectedCanConsume );

    private void TestOneLicenseSource(
        ILicenseSource licenseSource,
        LicensedFeatures requiredFeatures,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var manager = this.CreateConsumptionManager( licenseSource );
        TestConsumption( manager, requiredFeatures, requiredNamespace, expectedCanConsume );
    }

    [Fact]
    public void NoLicenseAutoRegistersEvaluationLicense()
    {
        LicenseConsumptionManager manager = new( this.ServiceProvider );
        TestConsumption( manager, LicensedFeatures.MetalamaAspectFramework, false, true );
    }

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, true )]
    [InlineData( TestLicenses.PostSharpFramework, true )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.Caching, false )]
    [InlineData( TestLicenses.Logging, false )]
    [InlineData( TestLicenses.Model, false )]
    [InlineData( TestLicenses.Threading, false )]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, false )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, true )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, true )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, true )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaFreePersonal, true )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, true )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimatePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution, false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaFreeFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicensedProductFeatures.MetalamaFree, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, false )]
    [InlineData( TestLicenses.PostSharpFramework, true )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.Caching, false )]
    [InlineData( TestLicenses.Logging, false )]
    [InlineData( TestLicenses.Model, false )]
    [InlineData( TestLicenses.Threading, false )]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, false )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, false )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, false )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, false )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.MetalamaFreePersonal, false )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimatePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution, false )]
    public void NamespaceUnlimitedLicenseAllowsPostSharpFrameworkFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicensedProductFeatures.PostSharpFramework, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, false )]
    [InlineData( TestLicenses.PostSharpFramework, false )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.Caching, false )]
    [InlineData( TestLicenses.Logging, false )]
    [InlineData( TestLicenses.Model, false )]
    [InlineData( TestLicenses.Threading, false )]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, false )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, false )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, false )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, false )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.MetalamaFreePersonal, false )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimatePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution, false )]
    public void NamespaceUnlimitedLicenseAllowsPostSharpUltimateFeatures( string licenseKey, bool expectedCanConsume )
    => this.TestOneLicense( licenseKey, LicensedProductFeatures.PostSharpUltimate, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, false )]
    [InlineData( TestLicenses.PostSharpFramework, true )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.Caching, false )]
    [InlineData( TestLicenses.Logging, false )]
    [InlineData( TestLicenses.Model, false )]
    [InlineData( TestLicenses.Threading, false )]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, false )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, true )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, true )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, true )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaFreePersonal, false )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, true )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimatePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution, false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaStarterFeatures( string licenseKey, bool expectedCanConsume )
=> this.TestOneLicense( licenseKey, LicensedProductFeatures.MetalamaStarter, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, false )]
    [InlineData( TestLicenses.PostSharpFramework, true )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.Caching, false )]
    [InlineData( TestLicenses.Logging, false )]
    [InlineData( TestLicenses.Model, false )]
    [InlineData( TestLicenses.Threading, false )]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, false )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, false )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, true )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaFreePersonal, false )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, true )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimatePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution, false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaProfessionalFeatures( string licenseKey, bool expectedCanConsume )
=> this.TestOneLicense( licenseKey, LicensedProductFeatures.MetalamaProfessional, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, false )]
    [InlineData( TestLicenses.PostSharpFramework, true )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.Caching, false )]
    [InlineData( TestLicenses.Logging, false )]
    [InlineData( TestLicenses.Model, false )]
    [InlineData( TestLicenses.Threading, false )]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, false )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, false )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, true )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaFreePersonal, false )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, true )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimatePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreePersonal, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreeBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution, false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaUltimateFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicensedProductFeatures.MetalamaUltimate, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, TestLicenses.NamespaceLimitedPostSharpUltimateOpenSourceNamespace, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimatePersonal, TestLicenses.NamespaceLimitedMetalamaUltimateNamespace, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, TestLicenses.NamespaceLimitedMetalamaUltimateNamespace, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreePersonal, TestLicenses.NamespaceLimitedMetalamaFreeNamespace, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreeBusiness, TestLicenses.NamespaceLimitedMetalamaFreeNamespace, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, TestLicenses.NamespaceLimitedMetalamaUltimateRedistributionNamespace, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution, TestLicenses.NamespaceLimitedMetalamaUltimateRedistributionNamespace, true )]
    public void NamespaceLimitedLicenseAllowsMetalamaInSameNamespace( string licenseKey, string requiredNamespace, bool expectedCanConsume )
        => this.TestOneLicense(
            licenseKey,
            LicensedProductFeatures.MetalamaUltimate,
            requiredNamespace,
            expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimatePersonal )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreePersonal )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaFreeBusiness )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateCommercialRedistribution )]
    public void NamespaceLimitedLicenseForbidsMetalamaDifferentNamespace( string licenseKey )
    => this.TestOneLicense(
        licenseKey,
        LicensedProductFeatures.MetalamaUltimate,
        "Foo",
        false );

    [Fact]
    public void ProfessionalAndUltimateProvideSameFeatures()
    {
        // As soon as this assumption becomes false, uncomment Metalama Ultimate in the following tests.
        Assert.Equal( LicensedProductFeatures.MetalamaUltimate, LicensedProductFeatures.MetalamaProfessional );
    }

    [Theory]
    [InlineData( LicensedProductFeatures.PostSharpEssentials, false )]
    [InlineData( LicensedProductFeatures.Mvvm, false )]
    [InlineData( LicensedProductFeatures.Threading, false )]
    [InlineData( LicensedProductFeatures.Logging, false )]
    [InlineData( LicensedProductFeatures.Caching, false )]
    [InlineData( LicensedProductFeatures.PostSharpFramework, false )]
    [InlineData( LicensedProductFeatures.PostSharpUltimate, false )]
    [InlineData( LicensedProductFeatures.MetalamaFree, true )]
    [InlineData( LicensedProductFeatures.MetalamaStarter, false )]
    [InlineData( LicensedProductFeatures.MetalamaProfessional, false )]

    // [InlineData( LicensedProductFeatures.MetalamaUltimate, false )] See ProfessionalAndUltimateProvideSameFeatures test.
    public void SelfSignedMetalamaFreeLicenseAllowsFeatures( LicensedFeatures requiredFeatures, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenses.CreateMetalamaFreeLicense( this.ServiceProvider ), requiredFeatures, expectedCanConsume );

    [Theory]
    [InlineData( LicensedProductFeatures.PostSharpEssentials, false )]
    [InlineData( LicensedProductFeatures.Mvvm, false )]
    [InlineData( LicensedProductFeatures.Threading, false )]
    [InlineData( LicensedProductFeatures.Logging, false )]
    [InlineData( LicensedProductFeatures.Caching, false )]
    [InlineData( LicensedProductFeatures.PostSharpFramework, false )]
    [InlineData( LicensedProductFeatures.PostSharpUltimate, false )]
    [InlineData( LicensedProductFeatures.MetalamaFree, true )]
    [InlineData( LicensedProductFeatures.MetalamaStarter, true )]
    [InlineData( LicensedProductFeatures.MetalamaProfessional, true )]

    // [InlineData( LicensedProductFeatures.MetalamaUltimate, true )] See ProfessionalAndUltimateProvideSameFeatures test.
    public void SelfSignedMetalamaEvaluationLicenseAllowsFeatures( LicensedFeatures requiredFeatures, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenses.CreateMetalamaEvaluationLicense( this.ServiceProvider ), requiredFeatures, expectedCanConsume );

    [Theory]
    [InlineData( LicensedProductFeatures.PostSharpEssentials, true )]
    [InlineData( LicensedProductFeatures.Mvvm, true )]
    [InlineData( LicensedProductFeatures.Threading, true )]
    [InlineData( LicensedProductFeatures.Logging, true )]
    [InlineData( LicensedProductFeatures.Caching, true )]
    [InlineData( LicensedProductFeatures.PostSharpFramework, true )]
    [InlineData( LicensedProductFeatures.PostSharpUltimate, true )]
    [InlineData( LicensedProductFeatures.MetalamaFree, true )]
    [InlineData( LicensedProductFeatures.MetalamaStarter, true )]
    [InlineData( LicensedProductFeatures.MetalamaProfessional, true )]

    // [InlineData( LicensedProductFeatures.MetalamaUltimate, true )] See ProfessionalAndUltimateProvideSameFeatures test.
    public void UnattendedLicenseAllowsFeatures( LicensedFeatures requiredFeatures, bool expectedCanConsume )
        => this.TestOneLicenseSource( TestLicenses.CreateUnattendedLicenseSource(), requiredFeatures, expectedCanConsume );

    [Theory]
    [InlineData( LicensedProductFeatures.PostSharpEssentials, true )]
    [InlineData( LicensedProductFeatures.Mvvm, true )]
    [InlineData( LicensedProductFeatures.Threading, true )]
    [InlineData( LicensedProductFeatures.Logging, true )]
    [InlineData( LicensedProductFeatures.Caching, true )]
    [InlineData( LicensedProductFeatures.PostSharpFramework, true )]
    [InlineData( LicensedProductFeatures.PostSharpUltimate, true )]
    [InlineData( LicensedProductFeatures.MetalamaFree, true )]
    [InlineData( LicensedProductFeatures.MetalamaStarter, true )]
    [InlineData( LicensedProductFeatures.MetalamaProfessional, true )]

    // [InlineData( LicensedProductFeatures.MetalamaUltimate, true )] See ProfessionalAndUltimateProvideSameFeatures test.
    public void PreviewLicenseAllowsFeatures( LicensedFeatures requiredFeatures, bool expectedCanConsume )
        => this.TestOneLicenseSource( TestLicenses.CreatePreviewLicenseSource( true, 0 ), requiredFeatures, expectedCanConsume );
}