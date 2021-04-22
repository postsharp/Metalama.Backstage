using PostSharp.Backstage.Utilities;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing
{
    /// <summary>
    /// Features of PostSharp are grouped into licensed packages for the purpose of license checking.
    /// The names and granularity correspond to the NuGet packages.
    /// </summary>
    [Flags]
    public enum LicensedPackages : int
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

    internal static class LicensedProductPackages
    {
        public const LicensedPackages Community = LicensedPackages.Community | LicensedPackages.Common; 
        public const LicensedPackages Mvvm = LicensedPackages.Community | LicensedPackages.Common | LicensedPackages.Model | 
                                             LicensedPackages.Xaml | LicensedPackages.Aggregatable;
        public const LicensedPackages Threading = LicensedPackages.Community | LicensedPackages.Common | LicensedPackages.Threading | LicensedPackages.Aggregatable;
        public const LicensedPackages Logging = LicensedPackages.Community | LicensedPackages.Common | LicensedPackages.Diagnostics;
        public const LicensedPackages Caching = LicensedPackages.Community | LicensedPackages.Common | LicensedPackages.Caching;
        public const LicensedPackages Framework = LicensedPackages.Community | LicensedPackages.Common | LicensedPackages.Framework;
        public const LicensedPackages Ultimate = LicensedPackages.All;
        public const LicensedPackages Unattended = LicensedPackages.All & ~LicensedPackages.Diagnostics;
    }
    
    
    internal static class LicensedPackageExtensionsInternal
    {
        private const LicensedPackages embeddedLicenseEnabled = LicensedPackages.Framework;

        public static bool CanUseEmbeddedLicense( this LicensedPackages licensedPackage ) 
            => ( embeddedLicenseEnabled & licensedPackage ) == licensedPackage;

        public static int GetRank( this LicensedPackages licensedPackages )
            => BitSetHelper.CountBits( (int) licensedPackages );
    }

    public static class LicensedPackageExtensions
    {
        public static bool Includes( this LicensedPackages licensedPackages, LicensedPackages requirements )
        {
            return ( licensedPackages & requirements ) == requirements;
        }

        public static LicensedPackages RemoveRequirements( this LicensedPackages licensedPackages, LicensedPackages requirements )
        {
            return licensedPackages & ~requirements;
        }
    }

    public static class LicensedPackagesHelper {

        private static readonly Dictionary<string, LicensedPackages> requiredLicensedPackagesForLibraries = new Dictionary<string, LicensedPackages>( StringComparer.OrdinalIgnoreCase )
                                                                                                           {
                                                                                                               { "PostSharp.Patterns.Common", LicensedPackages.Common },
                                                                                                               { "PostSharp.Patterns.Aggregation", LicensedPackages.Aggregatable },
                                                                                                               { "PostSharp.Patterns.Caching", LicensedPackages.Caching },
                                                                                                               { "PostSharp.Patterns.Diagnostics", LicensedPackages.Diagnostics },
                                                                                                               { "PostSharp.Patterns.Model", LicensedPackages.Model },
                                                                                                               { "PostSharp.Patterns.Xaml", LicensedPackages.Xaml },
                                                                                                               { "PostSharp.Patterns.Threading", LicensedPackages.Threading }
                                                                                                           };


        /// <summary>
        /// Return required licensed packages for the given assembly name.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly</param>
        /// <returns>Required licensed packages if assemblyName has licensing requirements, otherwise LicensedPackage.None.</returns>
        public static LicensedPackages GetRequiredLicensedPackageForAssembly( string assemblyName )
        {
            if ( requiredLicensedPackagesForLibraries.TryGetValue( assemblyName, out LicensedPackages requiredPackages ) )
            {
                return requiredPackages;
            }

            return LicensedPackages.None;
        }

        public static IEnumerable<LicensedPackages> GetPresentFlags( this LicensedPackages licensedPackages )
        {
            int cursor = 1;
            while ( cursor <= (int)licensedPackages )
            {
                if ( ( (LicensedPackages)cursor & licensedPackages ) != 0 )
                {
                    yield return (LicensedPackages)cursor;
                }
                cursor <<= 1;
            }
        }

        internal static LicensedPackages DowngradeFrameworkToCommunity( this LicensedPackages licensedPackages, bool downgrade )
        {
            if ( licensedPackages == LicensedPackages.Framework && downgrade  )
            {
                return LicensedPackages.Community;
            }
            else
            {
                return licensedPackages;
            }
        }
    }
}
