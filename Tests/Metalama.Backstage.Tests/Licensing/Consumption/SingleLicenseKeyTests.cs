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

    private void TestOneLicense( string licenseKey, LicenseRequirement requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, requestedRequirement, "Foo", expectedCanConsume );

    private void TestOneLicense(
        string licenseKey,
        LicenseRequirement requestedRequirement,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var license = this.CreateLicense( licenseKey );
        var manager = this.CreateConsumptionManager( license );
        TestConsumption( manager, requestedRequirement, requiredNamespace, expectedCanConsume );
    }

    private void TestOneLicenseSource( ILicenseSource licenseSource, LicenseRequirement requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource( licenseSource, requestedRequirement, "Foo", expectedCanConsume );

    private void TestOneLicenseSource(
        ILicenseSource licenseSource,
        LicenseRequirement requestedRequirement,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var manager = this.CreateConsumptionManager( licenseSource );
        TestConsumption( manager, requestedRequirement, requiredNamespace, expectedCanConsume );
    }

    [Theory]
    [InlineData( LicenseRequirement.Free )]
    [InlineData( LicenseRequirement.Starter )]
    [InlineData( LicenseRequirement.Professional )]
    [InlineData( LicenseRequirement.Ultimate )]
    public void NoLicenseAutoRegistersEvaluationLicense( LicenseRequirement requestedRequirement )
    {
        LicenseConsumptionManager manager = new( this.ServiceProvider );
        TestConsumption( manager, requestedRequirement, false, true );
    }

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, true )]
    [InlineData( TestLicenses.PostSharpFramework, true )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, true )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, true )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, true )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaFreePersonal, true )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, true )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaFreeFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirement.Free, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, false )]
    [InlineData( TestLicenses.PostSharpFramework, true )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, true )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, true )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, true )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaFreePersonal, false )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaStarterFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirement.Starter, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, false )]
    [InlineData( TestLicenses.PostSharpFramework, true )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, false )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, true )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, true )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaFreePersonal, false )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaProfessionalFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirement.Professional, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpEssentials, false )]
    [InlineData( TestLicenses.PostSharpFramework, false )]
    [InlineData( TestLicenses.PostSharpUltimate, true )]
    [InlineData( TestLicenses.PostSharpEnterprise, true )]
    [InlineData( TestLicenses.PostSharpUltimateOpenSourceRedistribution, false )]
    [InlineData( TestLicenses.MetalamaStarterPersonal, false )]
    [InlineData( TestLicenses.MetalamaStarterBusiness, false )]
    [InlineData( TestLicenses.MetalamaProfessionalPersonal, false )]
    [InlineData( TestLicenses.MetalamaProfessionalBusiness, false )]
    [InlineData( TestLicenses.MetalamaUltimatePersonal, true )]
    [InlineData( TestLicenses.MetalamaUltimateBusiness, true )]
    [InlineData( TestLicenses.MetalamaFreePersonal, false )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaUltimateFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirement.Ultimate, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpUltimateOpenSourceRedistribution, TestLicenses.PostSharpUltimateOpenSourceRedistributionNamespace, true )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, TestLicenses.MetalamaUltimateRedistributionNamespace, true )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, TestLicenses.MetalamaUltimateRedistributionNamespace, true )]
    public void NamespaceLimitedLicenseAllowsMetalamaInSameNamespace( string licenseKey, string requiredNamespace, bool expectedCanConsume )
        => this.TestOneLicense(
            licenseKey,
            LicenseRequirement.Ultimate,
            requiredNamespace,
            expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpUltimateOpenSourceRedistribution )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution )]
    public void NamespaceLimitedLicenseForbidsMetalamaInArbitraryNamespace( string licenseKey )
        => this.TestOneLicense(
            licenseKey,
            LicenseRequirement.Ultimate,
            "Foo",
            false );

    [Theory]
    [InlineData( LicenseRequirement.Free, true )]
    [InlineData( LicenseRequirement.Starter, false )]
    [InlineData( LicenseRequirement.Professional, false )]
    [InlineData( LicenseRequirement.Ultimate, false )]
    public void UnsignedMetalamaFreeLicenseAllowsRequirements( LicenseRequirement requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenses.CreateMetalamaFreeLicense( this.ServiceProvider ), requestedRequirement, expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirement.Free, true )]
    [InlineData( LicenseRequirement.Starter, true )]
    [InlineData( LicenseRequirement.Professional, true )]
    [InlineData( LicenseRequirement.Ultimate, true )]
    public void UnsignedMetalamaEvaluationLicenseAllowsRequirements( LicenseRequirement requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenses.CreateMetalamaEvaluationLicense( this.ServiceProvider ), requestedRequirement, expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirement.Free, true )]
    [InlineData( LicenseRequirement.Starter, true )]
    [InlineData( LicenseRequirement.Professional, true )]
    [InlineData( LicenseRequirement.Ultimate, true )]
    public void UnattendedLicenseAllowsRequirements( LicenseRequirement requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource( TestLicenses.CreateUnattendedLicenseSource(), requestedRequirement, expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirement.Free, true )]
    [InlineData( LicenseRequirement.Starter, true )]
    [InlineData( LicenseRequirement.Professional, true )]
    [InlineData( LicenseRequirement.Ultimate, true )]
    public void PreviewLicenseAllowsRequirements( LicenseRequirement requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource( TestLicenses.CreatePreviewLicenseSource( true, 0 ), requestedRequirement, expectedCanConsume );
}