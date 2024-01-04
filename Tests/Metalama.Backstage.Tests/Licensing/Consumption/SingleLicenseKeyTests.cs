// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.Licensing.Consumption;

public class SingleLicenseKeyTests : LicenseConsumptionManagerTestsBase
{
    public SingleLicenseKeyTests( ITestOutputHelper logger )
        : base( logger ) { }

    private void TestOneLicense( string licenseKeyName, LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicense( licenseKeyName, requestedRequirement, "Foo", expectedCanConsume );

    private void TestOneLicense(
        string licenseKeyName,
        LicenseRequirementTestEnum requestedRequirement,
        string requiredNamespace,
        bool expectedCanConsume )
    {
        var license = this.CreateLicense( licenseKeyName );
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

    [Theory]
    [InlineData( nameof(TestLicenseKeys.PostSharpEssentials), false )]
    [InlineData( nameof(TestLicenseKeys.PostSharpFramework), true )]
    [InlineData( nameof(TestLicenseKeys.PostSharpUltimate), true )]
    [InlineData( nameof(TestLicenseKeys.PostSharpEnterprise), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaStarterPersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalPersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaFreeBusiness), true )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaFreeFeatures( string licenseKeyName, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenseKeys.GetLicenseKey( licenseKeyName ), LicenseRequirementTestEnum.Free, expectedCanConsume );

    [Theory]
    [InlineData( nameof(TestLicenseKeys.PostSharpEssentials), false )]
    [InlineData( nameof(TestLicenseKeys.PostSharpFramework), true )]
    [InlineData( nameof(TestLicenseKeys.PostSharpUltimate), true )]
    [InlineData( nameof(TestLicenseKeys.PostSharpEnterprise), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaStarterPersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalPersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), false )]
    [InlineData( nameof(TestLicenseKeys.MetalamaFreeBusiness), false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaStarterFeatures( string licenseKeyName, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenseKeys.GetLicenseKey( licenseKeyName ), LicenseRequirementTestEnum.Starter, expectedCanConsume );

    [Theory]
    [InlineData( nameof(TestLicenseKeys.PostSharpEssentials), false )]
    [InlineData( nameof(TestLicenseKeys.PostSharpFramework), true )]
    [InlineData( nameof(TestLicenseKeys.PostSharpUltimate), true )]
    [InlineData( nameof(TestLicenseKeys.PostSharpEnterprise), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaStarterPersonal), false )]
    [InlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), false )]
    [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalPersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), false )]
    [InlineData( nameof(TestLicenseKeys.MetalamaFreeBusiness), false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaProfessionalFeatures( string licenseKeyName, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenseKeys.GetLicenseKey( licenseKeyName ), LicenseRequirementTestEnum.Professional, expectedCanConsume );

    [Theory]
    [InlineData( nameof(TestLicenseKeys.PostSharpEssentials), false )]
    [InlineData( nameof(TestLicenseKeys.PostSharpFramework), false )]
    [InlineData( nameof(TestLicenseKeys.PostSharpUltimate), true )]
    [InlineData( nameof(TestLicenseKeys.PostSharpEnterprise), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaStarterPersonal), false )]
    [InlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), false )]
    [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalPersonal), false )]
    [InlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), false )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonal), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), false )]
    [InlineData( nameof(TestLicenseKeys.MetalamaFreeBusiness), false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaUltimateFeatures( string licenseKeyName, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenseKeys.GetLicenseKey( licenseKeyName ), LicenseRequirementTestEnum.Ultimate, expectedCanConsume );

    [Theory]
    [InlineData(
        nameof(TestLicenseKeys.PostSharpUltimateOpenSourceRedistribution),
        TestLicenseKeys.PostSharpUltimateOpenSourceRedistributionNamespace,
        true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution), TestLicenseKeys.MetalamaUltimateRedistributionNamespace, true )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateCommercialRedistribution), TestLicenseKeys.MetalamaUltimateRedistributionNamespace, true )]
    public void NamespaceLimitedLicenseAllowsMetalamaInSameNamespace( string licenseKeyName, string requiredNamespace, bool expectedCanConsume )
        => this.TestOneLicense(
            TestLicenseKeys.GetLicenseKey( licenseKeyName ),
            LicenseRequirementTestEnum.Ultimate,
            requiredNamespace,
            expectedCanConsume );

    [Theory]
    [InlineData( nameof(TestLicenseKeys.PostSharpUltimateOpenSourceRedistribution) )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution) )]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimateCommercialRedistribution) )]
    public void RedistributionLicenseAllowsMetalamaInArbitraryNamespace( string licenseKeyName )
        => this.TestOneLicense(
            TestLicenseKeys.GetLicenseKey( licenseKeyName ),
            LicenseRequirementTestEnum.Ultimate,
            "Foo",
            true );

    [Theory]
    [InlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonalProjectBound) )]
    public void ProjectBoundLicenseForbidsMetalamaInArbitraryNamespace( string licenseKeyName )
        => this.TestOneLicense(
            TestLicenseKeys.GetLicenseKey( licenseKeyName ),
            LicenseRequirementTestEnum.Ultimate,
            "Foo",
            false );

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, true )]
    [InlineData( LicenseRequirementTestEnum.Starter, false )]
    [InlineData( LicenseRequirementTestEnum.Professional, false )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, false )]
    public void UnsignedMetalamaFreeLicenseAllowsRequirements( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenseFactory.CreateMetalamaFreeLicense( this.ServiceProvider ), requestedRequirement, expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, true )]
    [InlineData( LicenseRequirementTestEnum.Starter, true )]
    [InlineData( LicenseRequirementTestEnum.Professional, true )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, true )]
    public void UnsignedMetalamaEvaluationLicenseAllowsRequirements( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicense( TestLicenseFactory.CreateMetalamaEvaluationLicense( this.ServiceProvider ), requestedRequirement, expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, true )]
    [InlineData( LicenseRequirementTestEnum.Starter, true )]
    [InlineData( LicenseRequirementTestEnum.Professional, true )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, true )]
    public void UnattendedLicenseAllowsRequirements( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource( TestLicenseFactory.CreateUnattendedLicenseSource(), requestedRequirement, expectedCanConsume );
}