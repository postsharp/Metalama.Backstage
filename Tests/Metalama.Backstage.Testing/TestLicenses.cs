// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable StringLiteralTypo

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;

#pragma warning disable SA1203

namespace Metalama.Backstage.Testing;

public static class TestLicenses
{
    static TestLicenses()
    {
        var kvUri = "https://testserviceskeyvault.vault.azure.net/";
        var client = new SecretClient( new Uri( kvUri ), new DefaultAzureCredential() );

        string GetLicenseKey( string keyName ) => client.GetSecret( $"TestLicenseKey{keyName}" ).Value.Value;

        PostSharpEssentials = GetLicenseKey( nameof(PostSharpEssentials) );
        PostSharpFramework = GetLicenseKey( nameof(PostSharpFramework) );
        PostSharpUltimate = GetLicenseKey( nameof(PostSharpUltimate) );
        PostSharpEnterprise = GetLicenseKey( nameof(PostSharpEnterprise) );
        PostSharpUltimateOpenSourceRedistribution = GetLicenseKey( nameof(PostSharpUltimateOpenSourceRedistribution) );
        MetalamaFreePersonal = GetLicenseKey( nameof(MetalamaFreePersonal) );
        MetalamaFreeBusiness = GetLicenseKey( nameof(MetalamaFreeBusiness) );
        MetalamaStarterPersonal = GetLicenseKey( nameof(MetalamaStarterPersonal) );
        MetalamaStarterBusiness = GetLicenseKey( nameof(MetalamaStarterBusiness) );
        MetalamaProfessionalPersonal = GetLicenseKey( nameof(MetalamaProfessionalPersonal) );
        MetalamaProfessionalBusiness = GetLicenseKey( nameof(MetalamaProfessionalBusiness) );
        MetalamaUltimatePersonal = GetLicenseKey( nameof(MetalamaUltimatePersonal) );
        MetalamaUltimateBusiness = GetLicenseKey( nameof(MetalamaUltimateBusiness) );
        MetalamaUltimateBusinessNotAuditable = GetLicenseKey( nameof(MetalamaUltimateBusinessNotAuditable) );
        MetalamaUltimateOpenSourceRedistribution = GetLicenseKey( nameof(MetalamaUltimateOpenSourceRedistribution) );
        MetalamaUltimateCommercialRedistribution = GetLicenseKey( nameof(MetalamaUltimateOpenSourceRedistribution) );
        MetalamaUltimatePersonalProjectBound = GetLicenseKey( nameof( MetalamaUltimatePersonalProjectBound ) );
        MetalamaUltimateOpenSourceRedistributionForIntegrationTests = GetLicenseKey( nameof(MetalamaUltimateOpenSourceRedistributionForIntegrationTests) );
    }

    public static readonly string PostSharpEssentials;

    public static readonly string PostSharpFramework;

    public static readonly string PostSharpUltimate;

    public static readonly string PostSharpEnterprise;

    public const string PostSharpUltimateOpenSourceRedistributionNamespace = "Oss";

    public static readonly string PostSharpUltimateOpenSourceRedistribution;

    public static readonly string MetalamaFreePersonal;

    public static readonly string MetalamaFreeBusiness;

    public static readonly string MetalamaStarterPersonal;

    public static readonly string MetalamaStarterBusiness;

    public static readonly string MetalamaProfessionalPersonal;

    public static readonly string MetalamaProfessionalBusiness;

    public static readonly string MetalamaUltimatePersonal;

    public static readonly string MetalamaUltimateBusiness;

    public static readonly string MetalamaUltimateBusinessNotAuditable;

    public const string MetalamaUltimateRedistributionNamespace = "RedistributionTests.TargetNamespace";

    public static readonly string MetalamaUltimateOpenSourceRedistribution;

    public static readonly string MetalamaUltimateCommercialRedistribution;

    public const string MetalamaUltimateProjectBoundProjectName = "ProjectBoundTestsProject";

    public static readonly string MetalamaUltimatePersonalProjectBound;

    // This is to be used in Metalama Integration tests, where the test dependency has a random assembly name.
    public const string MetalamaUltimateOpenSourceRedistributionForIntegrationTestsNamespace = "dependency_XXXXXXXXXXXXXXXX";

    public static readonly string MetalamaUltimateOpenSourceRedistributionForIntegrationTests;

    public static readonly DateTime SubscriptionExpirationDate = new( 2050, 1, 1 );
}