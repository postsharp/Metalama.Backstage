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

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpEssentials), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpFramework), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpUltimate), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpEnterprise), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaStarterPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaProfessionalPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaFreeBusiness), true )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaFreeFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Free, expectedCanConsume );

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpEssentials), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpFramework), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpUltimate), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpEnterprise), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaStarterPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaProfessionalPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaFreeBusiness), false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaStarterFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Starter, expectedCanConsume );

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpEssentials), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpFramework), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpUltimate), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpEnterprise), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaStarterPersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaProfessionalPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaFreeBusiness), false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaProfessionalFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Professional, expectedCanConsume );

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpEssentials), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpFramework), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpUltimate), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpEnterprise), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaStarterPersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaStarterBusiness), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaProfessionalPersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaProfessionalBusiness), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaFreePersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaFreeBusiness), false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaUltimateFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Ultimate, expectedCanConsume );

    [Theory]
    [TestLicensesInlineData(
        nameof(TestLicenseKeys.PostSharpUltimateOpenSourceRedistribution),
        TestLicenseKeys.PostSharpUltimateOpenSourceRedistributionNamespace,
        true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution), TestLicenseKeys.MetalamaUltimateRedistributionNamespace, true )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateCommercialRedistribution), TestLicenseKeys.MetalamaUltimateRedistributionNamespace, true )]
    public void NamespaceLimitedLicenseAllowsMetalamaInSameNamespace( string licenseKey, string requiredNamespace, bool expectedCanConsume )
        => this.TestOneLicense(
            licenseKey,
            LicenseRequirementTestEnum.Ultimate,
            requiredNamespace,
            expectedCanConsume );

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenseKeys.PostSharpUltimateOpenSourceRedistribution) )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateOpenSourceRedistribution) )]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimateCommercialRedistribution) )]
    public void RedistributionLicenseAllowsMetalamaInArbitraryNamespace( string licenseKey )
        => this.TestOneLicense(
            licenseKey,
            LicenseRequirementTestEnum.Ultimate,
            "Foo",
            true );

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenseKeys.MetalamaUltimatePersonalProjectBound) )]
    public void ProjectBoundLicenseForbidsMetalamaInArbitraryNamespace( string licenseKey )
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

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, true )]
    [InlineData( LicenseRequirementTestEnum.Starter, true )]
    [InlineData( LicenseRequirementTestEnum.Professional, true )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, true )]
    public void PreviewLicenseAllowsRequirementsForPreviewBeforeTimeBomb( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource( TestLicenseFactory.CreatePreviewLicenseSource( true, 0 ), requestedRequirement, expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, false )]
    [InlineData( LicenseRequirementTestEnum.Starter, false )]
    [InlineData( LicenseRequirementTestEnum.Professional, false )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, false )]
    public void PreviewLicenseDisallowsRequirementsForPreviewAfterTimeBomb( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource(
            TestLicenseFactory.CreatePreviewLicenseSource( true, PreviewLicenseSource.PreviewLicensePeriod + 1 ),
            requestedRequirement,
            expectedCanConsume );

    [Theory]
    [InlineData( LicenseRequirementTestEnum.Free, false )]
    [InlineData( LicenseRequirementTestEnum.Starter, false )]
    [InlineData( LicenseRequirementTestEnum.Professional, false )]
    [InlineData( LicenseRequirementTestEnum.Ultimate, false )]
    public void PreviewLicenseDisallowsRequirementsForNotPreviewBeforeTimeBomb( LicenseRequirementTestEnum requestedRequirement, bool expectedCanConsume )
        => this.TestOneLicenseSource(
            TestLicenseFactory.CreatePreviewLicenseSource( false, 0 ),
            requestedRequirement,
            expectedCanConsume );
}