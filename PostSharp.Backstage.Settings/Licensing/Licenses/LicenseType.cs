// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing.Licenses
{
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
        /// Commercial license (Enterprise edition).
        /// </summary>
        [Obsolete( "No longer supported." )]
        Enterprise = 15,

        /// <summary>
        /// Internal license for build servers.
        /// </summary>
        Unattended = 16,

        /// <summary>
        /// Internal license for developers building unmodified source code.
        /// </summary>
        Unmodified = 17,

        /// <summary>
        /// License limiting the number of enhanced types. This is used with Ultimate or with the aspect libraries.
        /// </summary>
        PerUsage = 18,

        // 255 is reserved as unknown for testing purposes
    }
}