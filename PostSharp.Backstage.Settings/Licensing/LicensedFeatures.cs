// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing
{
    /// <exclude />
    /// <summary>
    /// Enumeration of features supported by the licensing system.
    /// </summary>
    /// <remarks>
    /// This should be in sync with BusinessSystems\common\SharpCrafters.Internal.Services.LicenseGenerator\ProductConfiguration.cs.
    /// </remarks>
    [Flags]
    public enum LicensedFeatures : long
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Features of the Community Edition (former Essentials, Express or Starter).
        /// </summary>
        BasicFeatures = 0x1,

        /// <summary>
        /// All aspect features of the PostSharp Framework Edition (former Professional).
        /// </summary>
        ProfessionalFeatures = 0x2,

        /// <summary>
        /// Platform hosting.
        /// </summary>
        Hosting = 0x8,

        /// <summary>
        /// Sdk (custom tasks).
        /// </summary>
        Sdk = 0x10,
        
        /// <summary>
        /// All aspect features of the Ultimate Edition.
        /// </summary>
        UltimateFeatures = 0x20,

        PortableClassLibrary = 0x40,

        /// <summary>
        /// All aspect features of the Enterprise Edition.
        /// </summary>
        EnterpriseFeatures = 0x100,
        
        /// <summary>
        /// Product limited to .NET Core.
        /// </summary>
        NetCoreFeatures = 0x200,

        /// <summary>
        /// All features.
        /// </summary>
        All = -1,
    }
}