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
            if ( string.IsNullOrEmpty( allowedNamespace ) )
            {
                throw new ArgumentException( "Missing namespace.", nameof(allowedNamespace) );
            }

            this.AllowedNamespace = allowedNamespace;
            this.LicensedFeatures = licensedFeatures;
        }

        /// <summary>
        /// Check if <paramref name="requiredNamespace"/> is allowed by the constraint.
        /// </summary>
        /// <param name="requiredNamespace">The namespace or assembly name required by a license consumer.</param>
        /// <returns>A value indication if <paramref name="requiredNamespace"/> is allowed by the constraint.</returns>
        public bool AllowsNamespace( string requiredNamespace )
        {
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