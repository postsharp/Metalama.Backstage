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

    /// <summary>
    /// Maps a product feature to a license feature.
    /// </summary>
    [Serializable]
    [Obsolete("Use LicensedPackage.")]
    public class LicenseRequirement
    {
        /// <summary>
        /// Initializes a new <see cref="LicenseRequirement"/> with default description.
        /// </summary>
        /// <param name="features">Required features.</param>
        public LicenseRequirement( LicensedFeatures features )
            : this( features, features.ToString() )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="LicenseRequirement"/>.
        /// </summary>
        /// <param name="features">Required license features.</param>
        /// <param name="description">Product feature description.</param>
        public LicenseRequirement( LicensedFeatures features, string description ) : this( LicensedProduct.PostSharp30, (long)features, description )
        {

        }

        public LicenseRequirement( LicensedProduct product, long features, string description )
        {
            Product = product;
            Description = description;
            Features = features;
        }


        public LicensedProduct Product { get; }

        /// <summary>
        /// Gets the required license features.
        /// </summary>
        public long Features { get; }

        /// <summary>
        /// Gets the description of the product feature.
        /// </summary>
        public string Description { get; }

        public override string ToString()
        {
            if ( this.Product == LicensedProduct.PostSharp30 )
            {
                return $"{{LicenseRequirement Features={( (LicensedFeatures)this.Features )}, Description='{this.Description}'}}";
            }
            else
            {
                return $"{{LicenseRequirement Product={this.Product}, Description='{this.Description}'}}";
            }
        }
    }
}