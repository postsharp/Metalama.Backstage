// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Consumption.Sources;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.Licensing.Consumption;

public class SingleLicenseKeyTests : LicenseConsumptionManagerTestsBase
{
    public SingleLicenseKeyTests( ITestOutputHelper logger )
        : base( logger ) { }

    private void TestOneLicense( string licenseKey, LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, requestedRequirement, "Foo", expectedCanConsume );

    private void TestOneLicense(
        string licenseKey,
        LicenseRequirementTestEnum requestedRequirement,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var license = this.CreateLicense( licenseKey );
        var manager = this.CreateConsumptionManager( license );
        TestConsumption( manager, requestedRequirement, requiredNamespace, expectedCanConsume );
    }

    private void TestOneLicenseSource( ILicenseSource licenseSource, LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource( licenseSource, requestedRequirement, "Foo", expectedCanConsume );

    private void TestOneLicenseSource(
        ILicenseSource licenseSource,
        LicenseRequirementTestEnum requestedRequirement,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var manager = this.CreateConsumptionManager( licenseSource );
        TestConsumption( manager, requestedRequirement, requiredNamespace, expectedCanConsume );
    }

    public static IEnumerable<LicenseRequirement> GetAllRequirements() => new[]
    {
        LicenseRequirement.Free,
        LicenseRequirement.Starter,
        LicenseRequirement.Professional,
        LicenseRequirement.Ultimate
    };

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free )]
    [InlineData( LicenseRequirementTestEnum.Starter )]
    [InlineData( LicenseRequirementTestEnum.Professional )]
    [InlineData( LicenseRequirementTestEnum.Ultimate )]
    public void NoLicenseAutoRegistersEvaluationLicense( LicenseRequirementTestEnum requestedRequirement )
    {
        LicenseConsumptionManager manager = new( this.ServiceProvider );
        TestConsumption( manager, requestedRequirement, false, true );
    }

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
    [InlineData( TestLicenses.MetalamaFreePersonal, true )]
    [InlineData( TestLicenses.MetalamaFreeBusiness, true )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaFreeFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Free, expectedCanConsume );

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
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Starter, expectedCanConsume );

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
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Professional, expectedCanConsume );

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
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Ultimate, expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpUltimateOpenSourceRedistribution, TestLicenses.PostSharpUltimateOpenSourceRedistributionNamespace, true )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution, TestLicenses.MetalamaUltimateRedistributionNamespace, true )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution, TestLicenses.MetalamaUltimateRedistributionNamespace, true )]
    public void NamespaceLimitedLicenseAllowsMetalamaInSameNamespace( string licenseKey, string requiredNamespace, bool expectedCanConsume )
        => this.TestOneLicense(
            licenseKey,
            LicenseRequirementTestEnum.Ultimate,
            requiredNamespace,
            expectedCanConsume );

    [Theory]
    [InlineData( TestLicenses.PostSharpUltimateOpenSourceRedistribution )]
    [InlineData( TestLicenses.MetalamaUltimateOpenSourceRedistribution )]
    [InlineData( TestLicenses.MetalamaUltimateCommercialRedistribution )]
    public void NamespaceLimitedLicenseForbidsMetalamaInArbitraryNamespace( string licenseKey )
        => this.TestOneLicense(
            licenseKey,
            LicenseRequirementTestEnum.Ultimate,
            "Foo",
            false );

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, true )]
    [InlineData( LicenseRequirementTestEnum.Starter, false )]
    [InlineData( LicenseRequirementTestEnum.Professional, false )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, false )]
    public void UnsignedMetalamaFreeLicenseAllowsRequirements( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenses.CreateMetalamaFreeLicense( this.ServiceProvider ), requestedRequirement, expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, true )]
    [InlineData( LicenseRequirementTestEnum.Starter, true )]
    [InlineData( LicenseRequirementTestEnum.Professional, true )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, true )]
    public void UnsignedMetalamaEvaluationLicenseAllowsRequirements( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenses.CreateMetalamaEvaluationLicense( this.ServiceProvider ), requestedRequirement, expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, true )]
    [InlineData( LicenseRequirementTestEnum.Starter, true )]
    [InlineData( LicenseRequirementTestEnum.Professional, true )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, true )]
    public void UnattendedLicenseAllowsRequirements( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource( TestLicenses.CreateUnattendedLicenseSource(), requestedRequirement, expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, true )]
    [InlineData( LicenseRequirementTestEnum.Starter, true )]
    [InlineData( LicenseRequirementTestEnum.Professional, true )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, true )]
    public void PreviewLicenseAllowsRequirements( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource( TestLicenses.CreatePreviewLicenseSource( true, 0 ), requestedRequirement, expectedCanConsume );
}