// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable StringLiteralTypo

using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

#pragma warning disable SA1203

namespace Metalama.Backstage.Testing;

public static class TestLicenseKeys
{
    private static readonly Dictionary<string, string>? _testLicenseKeys;

    static TestLicenseKeys()
    {
        // This is the same file name as in PostSharp.Engineering.BuildTools.Utilities.TestLicensesCache.FetchOnPrepareCompleted method,
        // and has to be changed when the content should change.
        const string testLicenseKeysCacheFileName = "TestLicenseKeys1.g.props";

        var dummyApplicationInfo = new TestApplicationInfo( "DummyForTestLicenseKeys", false, "1.0", DateTime.UtcNow );
        var servicesOptions = new BackstageInitializationOptions( dummyApplicationInfo );
        var services = new ServiceProviderBuilder().AddBackstageServices( servicesOptions ).ServiceProvider;
        var applicationDataDirectory = services.GetRequiredBackstageService<IStandardDirectories>().ApplicationDataDirectory;
        var testLicenseKeysCacheFile = Path.Combine( applicationDataDirectory, "TestLicenseKeysCache", testLicenseKeysCacheFileName );

        if ( File.Exists( testLicenseKeysCacheFile ) )
        {
            var root = XElement.Load( testLicenseKeysCacheFile );

            _testLicenseKeys = root.XPathSelectElements( "//PropertyGroup/*" )
                .Select( e => (e.Name.LocalName, e.Value) )
                .Where( p => p.LocalName.EndsWith( "LicenseKey", StringComparison.OrdinalIgnoreCase ) )
                .ToDictionary( p => p.LocalName.Substring( 0, p.LocalName.Length - "LicenseKey".Length ), p => p.Value );
        }
    }

    private static string GetLicenseKey( string keyName )
        => _testLicenseKeys?.TryGetValue( keyName, out var key ) ?? false
            ? key
            : throw new InvalidOperationException(
                $"Uknown test license key: '{keyName}'. Test license keys may not have been restored by PostSharp.Engineering. Licensing tests are only available on PostSharp-owned devices and will always fail otherwise." );

    public static string PostSharpEssentials => GetLicenseKey( nameof(PostSharpEssentials) );

    public static string PostSharpFramework => GetLicenseKey( nameof(PostSharpFramework) );

    public static string PostSharpUltimate => GetLicenseKey( nameof(PostSharpUltimate) );

    public static string PostSharpEnterprise => GetLicenseKey( nameof(PostSharpEnterprise) );

    public const string PostSharpUltimateOpenSourceRedistributionNamespace = "Oss";

    public static string PostSharpUltimateOpenSourceRedistribution => GetLicenseKey( nameof(PostSharpUltimateOpenSourceRedistribution) );

    public static string MetalamaFreePersonal => GetLicenseKey( nameof(MetalamaFreePersonal) );

    public static string MetalamaFreeBusiness => GetLicenseKey( nameof(MetalamaFreeBusiness) );

    public static string MetalamaStarterPersonal => GetLicenseKey( nameof(MetalamaStarterPersonal) );

    public static string MetalamaStarterBusiness => GetLicenseKey( nameof(MetalamaStarterBusiness) );

    public static string MetalamaProfessionalPersonal => GetLicenseKey( nameof(MetalamaProfessionalPersonal) );

    public static string MetalamaProfessionalBusiness => GetLicenseKey( nameof(MetalamaProfessionalBusiness) );

    public static string MetalamaUltimatePersonal => GetLicenseKey( nameof(MetalamaUltimatePersonal) );

    public static string MetalamaUltimateBusiness => GetLicenseKey( nameof(MetalamaUltimateBusiness) );

    public static string MetalamaUltimateBusinessNotAuditable => GetLicenseKey( nameof(MetalamaUltimateBusinessNotAuditable) );

    public const string MetalamaUltimateRedistributionNamespace = "RedistributionTests.TargetNamespace";

    public static string MetalamaUltimateOpenSourceRedistribution => GetLicenseKey( nameof(MetalamaUltimateOpenSourceRedistribution) );

    public static string MetalamaUltimateCommercialRedistribution => GetLicenseKey( nameof(MetalamaUltimateCommercialRedistribution) );

    public const string MetalamaUltimateProjectBoundProjectName = "ProjectBoundTestsProject";

    public static string MetalamaUltimatePersonalProjectBound => GetLicenseKey( nameof(MetalamaUltimatePersonalProjectBound) );

    // This is to be used in Metalama Integration tests, where the test dependency has a random assembly name.
    public const string MetalamaUltimateOpenSourceRedistributionForIntegrationTestsNamespace = "dependency_XXXXXXXXXXXXXXXX";

    public static string MetalamaUltimateOpenSourceRedistributionForIntegrationTests
        => GetLicenseKey( nameof(MetalamaUltimateOpenSourceRedistributionForIntegrationTests) );

    public static readonly DateTime SubscriptionExpirationDate = new( 2050, 1, 1 );
}