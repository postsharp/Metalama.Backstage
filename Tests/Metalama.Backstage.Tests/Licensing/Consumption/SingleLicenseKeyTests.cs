// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Testing;
using System.Collections.Generic;
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

    public static IEnumerable<LicenseRequirement> GetAllRequirements()
        => new[] { LicenseRequirement.Free, LicenseRequirement.Starter, LicenseRequirement.Professional, LicenseRequirement.Ultimate };

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpEssentials), false )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpFramework), true )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimate), true )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpEnterprise), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimatePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreeBusiness), true )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaFreeFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Free, expectedCanConsume );

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpEssentials), false )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpFramework), true )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimate), true )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpEnterprise), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimatePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreePersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreeBusiness), false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaStarterFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Starter, expectedCanConsume );

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpEssentials), false )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpFramework), true )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimate), true )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpEnterprise), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterPersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterBusiness), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalPersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimatePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreePersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreeBusiness), false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaProfessionalFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Professional, expectedCanConsume );

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpEssentials), false )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpFramework), false )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimate), true )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpEnterprise), true )]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimateOpenSourceRedistribution), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterPersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaStarterBusiness), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalPersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaProfessionalBusiness), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimatePersonal), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateBusiness), true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreePersonal), false )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaFreeBusiness), false )]
    public void NamespaceUnlimitedLicenseAllowsMetalamaUltimateFeatures( string licenseKey, bool expectedCanConsume )
        => this.TestOneLicense( licenseKey, LicenseRequirementTestEnum.Ultimate, expectedCanConsume );

    [Theory]
    [TestLicensesInlineData(
        nameof(TestLicenses.PostSharpUltimateOpenSourceRedistribution),
        TestLicenses.PostSharpUltimateOpenSourceRedistributionNamespace,
        true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateOpenSourceRedistribution), TestLicenses.MetalamaUltimateRedistributionNamespace, true )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateCommercialRedistribution), TestLicenses.MetalamaUltimateRedistributionNamespace, true )]
    public void NamespaceLimitedLicenseAllowsMetalamaInSameNamespace( string licenseKey, string requiredNamespace, bool expectedCanConsume )
        => this.TestOneLicense(
            licenseKey,
            LicenseRequirementTestEnum.Ultimate,
            requiredNamespace,
            expectedCanConsume );

    [Theory]
    [TestLicensesInlineData( nameof(TestLicenses.PostSharpUltimateOpenSourceRedistribution) )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateOpenSourceRedistribution) )]
    [TestLicensesInlineData( nameof(TestLicenses.MetalamaUltimateCommercialRedistribution) )]
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