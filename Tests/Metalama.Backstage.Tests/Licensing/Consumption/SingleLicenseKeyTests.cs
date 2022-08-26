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
        TestConsumption( manager, LicensedFeatures.MetalamaAspects, false, true );
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
    [InlineData( TestLicenses.MetalamaStarterBusiness, true )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimateEssentials, true )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaEssentialsFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicensedProductFeatures.MetalamaEssentials, expectedCanConsume );

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
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
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
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
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
    [InlineData( TestLicenses.MetalamaStarterBusiness, true )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
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
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
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
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaUltimateFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicensedProductFeatures.MetalamaUltimate, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource, TestLicenses.NamespaceLimitedPostSharpUltimateOpenSourceNamespace, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness, TestLicenses.NamespaceLimitedMetalamaUltimateBusinessNamespace, true )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials, TestLicenses.NamespaceLimitedMetalamaUltimateEssentialsNamespace, false )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution, TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistributionNamespace, true )]
    public void NamespaceLimitedLicenseAllowsMetalamaInSameNamespace( string licenseKey, string requiredNamespace, bool expectedCanConsume )
        => this.TestOneLicense(
            licenseKey,
            LicensedProductFeatures.MetalamaUltimate,
            requiredNamespace,
            expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.NamespaceLimitedPostSharpUltimateOpenSource )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateBusiness )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateEssentials )]
    [InlineData( TestLicenses.NamespaceLimitedMetalamaUltimateOpenSourceRedistribution )]
    public void NamespaceLimitedLicenseForbidsMetalamaDifferentNamespace( string licenseKey )
    => this.TestOneLicense(
        licenseKey,
        LicensedProductFeatures.MetalamaUltimate,
        "Foo",
        false );

    [Theory]
    [InlineData( LicensedProductFeatures.PostSharpEssentials, false )]
    [InlineData( LicensedProductFeatures.Mvvm, false )]
    [InlineData( LicensedProductFeatures.Threading, false )]
    [InlineData( LicensedProductFeatures.Logging, false )]
    [InlineData( LicensedProductFeatures.Caching, false )]
    [InlineData( LicensedProductFeatures.PostSharpFramework, false )]
    [InlineData( LicensedProductFeatures.PostSharpUltimate, false )]
    [InlineData( LicensedProductFeatures.MetalamaEssentials, true )]
    [InlineData( LicensedProductFeatures.MetalamaStarter, false )]
    [InlineData( LicensedProductFeatures.MetalamaProfessional, false )]
#pragma warning disable xUnit1025 // InlineData should be unique within the Theory it belongs to
    [InlineData( LicensedProductFeatures.MetalamaUltimate, false )]
#pragma warning restore xUnit1025 // InlineData should be unique within the Theory it belongs to
    public void SelfSignedMetalamaEssentialsLicenseAllowsFeatures( LicensedFeatures requiredFeatures, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenses.CreateMetalamaEssentialsLicense( this.ServiceProvider ), requiredFeatures, expectedCanConsume );

    [Theory]
    [InlineData( LicensedProductFeatures.PostSharpEssentials, false )]
    [InlineData( LicensedProductFeatures.Mvvm, false )]
    [InlineData( LicensedProductFeatures.Threading, false )]
    [InlineData( LicensedProductFeatures.Logging, false )]
    [InlineData( LicensedProductFeatures.Caching, false )]
    [InlineData( LicensedProductFeatures.PostSharpFramework, false )]
    [InlineData( LicensedProductFeatures.PostSharpUltimate, false )]
    [InlineData( LicensedProductFeatures.MetalamaEssentials, true )]
    [InlineData( LicensedProductFeatures.MetalamaStarter, true )]
    [InlineData( LicensedProductFeatures.MetalamaProfessional, true )]
#pragma warning disable xUnit1025 // InlineData should be unique within the Theory it belongs to
    [InlineData( LicensedProductFeatures.MetalamaUltimate, true )]
#pragma warning restore xUnit1025 // InlineData should be unique within the Theory it belongs to
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
    [InlineData( LicensedProductFeatures.MetalamaEssentials, true )]
    [InlineData( LicensedProductFeatures.MetalamaStarter, true )]
    [InlineData( LicensedProductFeatures.MetalamaProfessional, true )]
#pragma warning disable xUnit1025 // InlineData should be unique within the Theory it belongs to
    [InlineData( LicensedProductFeatures.MetalamaUltimate, true )]
#pragma warning restore xUnit1025 // InlineData should be unique within the Theory it belongs to
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
    [InlineData( LicensedProductFeatures.MetalamaEssentials, true )]
    [InlineData( LicensedProductFeatures.MetalamaStarter, true )]
    [InlineData( LicensedProductFeatures.MetalamaProfessional, true )]
#pragma warning disable xUnit1025 // InlineData should be unique within the Theory it belongs to
    [InlineData( LicensedProductFeatures.MetalamaUltimate, true )]
#pragma warning restore xUnit1025 // InlineData should be unique within the Theory it belongs to
    public void PreviewLicenseAllowsFeatures( LicensedFeatures requiredFeatures, bool expectedCanConsume )
        => this.TestOneLicenseSource( TestLicenses.CreatePreviewLicense( true, 0 ), requiredFeatures, expectedCanConsume );
}