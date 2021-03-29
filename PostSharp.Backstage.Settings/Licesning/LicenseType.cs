// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace PostSharp.Backstage.Licensing
{
#pragma warning disable CA1027 // Mark enums with FlagsAttribute
#pragma warning disable CA1028 // Enum Storage should be Int32
    /// <exclude />
    /// <summary>
    /// Types of licenses.
    /// </summary>
    /// <remarks>
    /// This should be in sync with PostSharp\Public\Core\PostSharp.Compiler.Settings\Extensibility\Licensing\ReportedLicenseType.cs
    /// and BusinessSystems\common\SharpCrafters.Internal.Services.LicenseGenerator\ProductConfiguration.cs.
    /// 
    /// Obsolete license types do not need to be included here
    /// and reported license types representing products and special cases should not be included here.
    /// </remarks>
    public enum LicenseType : byte

    {
        /// <summary>
        /// No license.
        /// </summary>
        None = 0,

        /// <summary>
        /// Community license (Essentials, Express or Starter).
        /// </summary>
        Community = 1,

        [Obsolete( "Use Community" )]
        Starter = Community,

        /// <summary>
        /// PerUser license.
        /// </summary>
        PerUser = 2,

        [Obsolete( "Use PerUser" )]
        Commercial = PerUser,

        /// <summary>
        /// Site license.
        /// </summary>
        Site = 3,

        /// <summary>
        /// Global license.
        /// </summary>
        Global = 4,

        /// <summary>
        /// Evaluation license.
        /// </summary>
        Evaluation = 5,

        /// <summary>
        /// Limited redistribution license.
        /// </summary>
        OpenSourceRedistribution = 6,
        LimitedRedistribution = OpenSourceRedistribution,

        /// <summary>
        /// Academic license.
        /// </summary>
        Academic = 8,

        /// <summary>
        /// Redistribution (with contract).
        /// </summary>
        CommercialRedistribution = 12,

        /// <summary>
        /// Anonymous license.
        /// </summary>
        [Obsolete( "No longer supported." )]
        Anonymous = 13,

        /// <summary>
        /// Commercial license (A pattern library, or PostSharp Framework (former Professional edition)).
        /// </summary>
        [Obsolete( "Use LicenseType.PerUser and LicensedProduct.Framework" )]
        Professional = 14,

        /// <summary>
        /// Commercial license (Enterprise edition)
        /// </summary>
        [Obsolete( "No longer supported." )]
        Enterprise = 15,

        /// <summary>
        /// Internal license for build servers
        /// </summary>
        Unattended = 16,

        /// <summary>
        /// Internal license for developers building unmodified source code
        /// </summary>
        Unmodified = 17,

        /// <summary>
        /// License limiting the number of enhanced types. This is used with Ultimate or with the aspect libraries.
        /// </summary>
        PerUsage = 18,

        // 255 is reserved as unknown for testing purposes
    }

    public static class LicenseTypeExtensions
    {
        public static string Format( this LicenseType licenseType )
        {
            switch ( licenseType )
            {
                
                case LicenseType.Evaluation:
                    return "Evaluation License";
                    
#pragma warning disable 618
                case LicenseType.Anonymous:
                    return "Anonymous License";
#pragma warning restore 618

                default:
                    // We don't want to display the license type for other licenses, because there may be
                    // a mismatch between what we sell (i.e. what is represented in the CRM and in the license certificate)
                    // and what is serialized into the license key.
                    return null;
            }
        }

#pragma warning disable 618
        public static LicenseType GetBestLicense( this IEnumerable<LicenseType> licenseTypes )
        {
            IList<LicenseType> types = licenseTypes as IList<LicenseType> ?? licenseTypes.ToList();

            if ( types.Any( l => l == LicenseType.Enterprise ) )
                return LicenseType.Enterprise;

            if ( types.Any( l => l == LicenseType.CommercialRedistribution ) )
                return LicenseType.CommercialRedistribution;

            if ( types.Any( l => l == LicenseType.PerUser ) )
                return LicenseType.PerUser;
            
            if ( types.Any( l => l == LicenseType.PerUsage ) )
                return LicenseType.PerUsage;

            if ( types.Any( l => l == LicenseType.Community ) )
                return LicenseType.Community;

            if ( types.Any( l => l == LicenseType.Global ) )
                return LicenseType.Global;

            if ( types.Any( l => l == LicenseType.OpenSourceRedistribution ) )
                return LicenseType.OpenSourceRedistribution;

            if ( types.Any( l => l == LicenseType.Site ) )
                return LicenseType.Site;

            if ( types.Any( l => l == LicenseType.Academic ) )
                return LicenseType.Academic;

            if ( types.Any( l => l == LicenseType.Evaluation ) )
                return LicenseType.Evaluation;

            if (types.Any(l => l == LicenseType.Unattended))
                return LicenseType.Unattended;

            if ( types.Any( l => l == LicenseType.Unmodified ) )
                return LicenseType.Unmodified;

            if ( types.Any( l => l == LicenseType.Academic ) )
                return LicenseType.Academic;

            return LicenseType.None;
        }
#pragma warning restore 618

        public static bool IsBetterThan( this LicenseType license1, LicenseType license2 )
        {
            return new[] {license1, license2}.GetBestLicense() == license1;
        }

        public static bool IsUserLicense( this LicenseType licenseType )
        {
#pragma warning disable 618
            switch ( licenseType )
            {
                case LicenseType.Community:
                case LicenseType.PerUser:
                case LicenseType.Site:
                case LicenseType.Global:
                case LicenseType.Evaluation:
                case LicenseType.Unattended:
                case LicenseType.Unmodified:
                case LicenseType.Academic:
                case LicenseType.Anonymous:
                case LicenseType.Enterprise:
                case LicenseType.PerUsage:
                    return true;
                default:
                    return false;
            }
#pragma warning restore 618
        }
    }
}