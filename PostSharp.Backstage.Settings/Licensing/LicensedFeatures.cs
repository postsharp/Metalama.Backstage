using PostSharp.Backstage.Utilities;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing
{
    /// <summary>
    /// Features of PostSharp for the purpose of license checking.
    /// The names and granularity correspond to the NuGet packages.
    /// </summary>
    [Flags]
    public enum LicensedFeatures : int
    {
        None = 0,

        // Some features are free in the Community Edition (e.g. non-semantic OnMethodBoundaryAspect), but those transformations
        // also need at least some license to be loaded. Licenses can only be loaded if a requirement is present.
        Community = 1,
        Common = 1 << 1,
        Framework = 1 << 2,
        Threading = 1 << 3,
        Model = 1 << 4,
        Xaml = 1 << 5,
        Aggregatable = 1 << 6,
        Diagnostics = 1 << 7,
        Caching = 1 << 8,
        Caravela = 1 << 9,

        All = int.MaxValue,
    }

    internal static class LicensedProductFeatures
    {
        public const LicensedFeatures Community = LicensedFeatures.Community | LicensedFeatures.Common; 
        public const LicensedFeatures Mvvm = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Model | 
                                             LicensedFeatures.Xaml | LicensedFeatures.Aggregatable;
        public const LicensedFeatures Threading = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Threading | LicensedFeatures.Aggregatable;
        public const LicensedFeatures Logging = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Diagnostics;
        public const LicensedFeatures Caching = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Caching;
        public const LicensedFeatures Framework = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Framework;
        public const LicensedFeatures Ultimate = LicensedFeatures.All;
        public const LicensedFeatures Unattended = LicensedFeatures.All & ~LicensedFeatures.Diagnostics;
    }
    
    
    internal static class LicensedFeaturesExtensionsInternal
    {
        private const LicensedFeatures embeddedLicenseEnabled = LicensedFeatures.Framework;

        public static bool CanUseEmbeddedLicense( this LicensedFeatures licensedFeatures ) 
            => ( embeddedLicenseEnabled & licensedFeatures ) == licensedFeatures;

        public static int GetRank( this LicensedFeatures licensedFeatures )
            => BitSetHelper.CountBits( (int) licensedFeatures );
    }

    public static class LicensedFeaturesExtensions
    {
        public static bool Includes( this LicensedFeatures licensedFeatures, LicensedFeatures requirements )
        {
            return ( licensedFeatures & requirements ) == requirements;
        }

        public static LicensedFeatures RemoveRequirements( this LicensedFeatures licensedFeatures, LicensedFeatures requirements )
        {
            return licensedFeatures & ~requirements;
        }
    }

    public static class LicensedFeaturesHelper {

        private static readonly Dictionary<string, LicensedFeatures> requiredLicensedFeaturesForLibraries = new Dictionary<string, LicensedFeatures>( StringComparer.OrdinalIgnoreCase )
                                                                                                           {
                                                                                                               { "PostSharp.Patterns.Common", LicensedFeatures.Common },
                                                                                                               { "PostSharp.Patterns.Aggregation", LicensedFeatures.Aggregatable },
                                                                                                               { "PostSharp.Patterns.Caching", LicensedFeatures.Caching },
                                                                                                               { "PostSharp.Patterns.Diagnostics", LicensedFeatures.Diagnostics },
                                                                                                               { "PostSharp.Patterns.Model", LicensedFeatures.Model },
                                                                                                               { "PostSharp.Patterns.Xaml", LicensedFeatures.Xaml },
                                                                                                               { "PostSharp.Patterns.Threading", LicensedFeatures.Threading }
                                                                                                           };


        /// <summary>
        /// Return required licensed features for the given assembly name.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly</param>
        /// <returns>Required licensed features if assemblyName has licensing requirements, otherwise LicensedFeatures.None.</returns>
        public static LicensedFeatures GetRequiredLicensedFeaturesForAssembly( string assemblyName )
        {
            if ( requiredLicensedFeaturesForLibraries.TryGetValue( assemblyName, out LicensedFeatures requiredFeatures ) )
            {
                return requiredFeatures;
            }

            return LicensedFeatures.None;
        }

        public static IEnumerable<LicensedFeatures> GetPresentFlags( this LicensedFeatures licensedFeatures )
        {
            int cursor = 1;
            while ( cursor <= (int)licensedFeatures )
            {
                if ( ( (LicensedFeatures)cursor & licensedFeatures ) != 0 )
                {
                    yield return (LicensedFeatures)cursor;
                }
                cursor <<= 1;
            }
        }

        internal static LicensedFeatures DowngradeFrameworkToCommunity( this LicensedFeatures licensedFeatures, bool downgrade )
        {
            if ( licensedFeatures == LicensedFeatures.Framework && downgrade  )
            {
                return LicensedFeatures.Community;
            }
            else
            {
                return licensedFeatures;
            }
        }
    }
}
