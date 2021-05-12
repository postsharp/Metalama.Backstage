// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Licensed features limited by a namespace constraint.
    /// </summary>
    internal class NamespaceLimitedLicenseFeatures
    {
        /// <summary>
        /// Gets the namespace constraint.
        /// </summary>
        public NamespaceConstraint Constraint { get; }

        /// <summary>
        /// Gets or sets the licensed features limited by the namespace constraint.
        /// </summary>
        public LicensedFeatures Features { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceLimitedLicenseFeatures"/> class.
        /// </summary>
        /// <param name="allowedNamespace">The namespace allowed by a license.</param>
        /// <param name="features">The features allowed by a license limited by the namespace constraint.</param>
        public NamespaceLimitedLicenseFeatures( string allowedNamespace, LicensedFeatures features = LicensedFeatures.None )
        {
            this.Constraint = new( allowedNamespace );
            this.Features = features;
        }
    }
}
