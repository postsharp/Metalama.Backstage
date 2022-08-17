﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Backstage.Licensing.Consumption
{
    /// <summary>
    /// License namespace constraint.
    /// </summary>
    internal class LicenseNamespaceConstraint
    {
        /// <summary>
        /// Gets the namespace allowed by the license.
        /// </summary>
        public string AllowedNamespace { get; }

        /// <summary>
        /// Gets or sets the licensed features limited by the namespace constraint.
        /// </summary>
        public LicensedFeatures LicensedFeatures { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of aspects limited by this namespace constraint.
        /// </summary>
        public int MaxApsectsCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseNamespaceConstraint"/> class.
        /// </summary>
        /// <param name="allowedNamespace">The namespace allowed by the license.</param>
        /// <param name="licensedFeatures">The licensed features limited by the namespace constraint.</param>
        /// <param name="maxApsectsCount">The maximum number of aspects limited by this namespace constraint.</param>
        public LicenseNamespaceConstraint(
            string allowedNamespace,
            LicensedFeatures licensedFeatures = LicensedFeatures.None,
            int maxApsectsCount = 0 )
        {
            if ( string.IsNullOrEmpty( allowedNamespace ) )
            {
                throw new ArgumentException( "Missing namespace.", nameof( allowedNamespace ) );
            }

            this.AllowedNamespace = allowedNamespace;
            this.LicensedFeatures = licensedFeatures;
            this.MaxApsectsCount = maxApsectsCount;
        }

        /// <summary>
        /// Check if <paramref name="requiredNamespace"/> is allowed by the constraint.
        /// </summary>
        /// <param name="requiredNamespace">The namespace or assembly name required by a license consumer. <c>null</c> for requirements where namespace is not applicable.</param>
        /// <returns>A value indicating if <paramref name="requiredNamespace"/> is allowed by the constraint.</returns>
        public bool AllowsNamespace( string? requiredNamespace )
        {
            // Requirements where namespace is not applicable are allowed regardless of license namespace constraints.
            if ( requiredNamespace == null )
            {
                return true;
            }

            if ( !requiredNamespace.StartsWith( this.AllowedNamespace, StringComparison.OrdinalIgnoreCase ) )
            {
                return false;
            }

            if ( requiredNamespace.Length == this.AllowedNamespace.Length )
            {
                return true;
            }

            var delimiter = requiredNamespace[this.AllowedNamespace.Length];

            if (

                // If there is not '.' after the namespace name,
                // it means the namespace name is different
                // and it only begins with the required name.
                delimiter == '.'

                // When we get a assembly full name,
                // there is a ',' after the short name.
                || delimiter == ',' )
            {
                return true;
            }

            return false;
        }
    }
}