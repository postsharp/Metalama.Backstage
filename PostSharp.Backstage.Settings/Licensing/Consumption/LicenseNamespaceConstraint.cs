// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing.Consumption
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
        /// Initializes a new instance of the <see cref="LicenseNamespaceConstraint"/> class.
        /// </summary>
        /// <param name="allowedNamespace">The namespace allowed by the license.</param>
        public LicenseNamespaceConstraint( string allowedNamespace, LicensedFeatures licensedFeatures = LicensedFeatures.None )
        {
            if (string.IsNullOrEmpty( allowedNamespace ))
            {
                throw new ArgumentException( "Missing namespace.", nameof(allowedNamespace) );
            }

            AllowedNamespace = allowedNamespace;
            LicensedFeatures = licensedFeatures;
        }

        /// <summary>
        /// Check if <paramref name="requiredNamespace"/> is allowed by the constraint.
        /// </summary>
        /// <param name="requiredNamespace">The namespace or assembly name required by a license consumer. <c>null</c> for requirements where namespace is not applicable.</param>
        /// <returns>A value indicating if <paramref name="requiredNamespace"/> is allowed by the constraint.</returns>
        public bool AllowsNamespace( string? requiredNamespace )
        {
            // Requirements where namespace is not applicable are allowed regardless of license namespace constraints.
            if (requiredNamespace == null)
            {
                return true;
            }

            if (!requiredNamespace.StartsWith( AllowedNamespace, StringComparison.OrdinalIgnoreCase ))
            {
                return false;
            }

            if (requiredNamespace.Length == AllowedNamespace.Length)
            {
                return true;
            }

            var delimiter = requiredNamespace[AllowedNamespace.Length];

            if (

                // If there is not '.' after the namespace name,
                // it means the namespace name is different
                // and it only begins with the required name.
                delimiter == '.'

                // When we get a assembly full name,
                // there is a ',' after the short name.
                || delimiter == ',')
            {
                return true;
            }

            return false;
        }
    }
}