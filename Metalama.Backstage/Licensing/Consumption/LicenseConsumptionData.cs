// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Licenses;
using System;

namespace Metalama.Backstage.Licensing.Consumption
{
    /// <summary>
    /// Information about a license relevant to licensed features consumption.
    /// </summary>
    public class LicenseConsumptionData
    {
        /// <summary>
        /// Gets features available by the license.
        /// </summary>
        public LicensedFeatures LicensedFeatures { get; }

        /// <summary>
        /// Gets the namespace constraint of the license.
        /// Gets <c>null</c> if there is no namespace constraint.
        /// </summary>
        public string? LicensedNamespace { get; }

        /// <summary>
        /// Gets the product licensed by the license.
        /// </summary>
        public LicensedProduct LicensedProduct { get; }

        /// <summary>
        /// Gets the type of the license.
        /// </summary>
        public LicenseType LicenseType { get; }

        /// <summary>
        /// Gets the displayable name of the license shown in diagnostics and trace messages.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the minimal PostSharp version this license can be used with.
        /// </summary>
        /// <remarks>
        /// Doesn't apply to products not based on PostSharp. (E.g. Metalama.)
        /// </remarks>
        public Version MinPostSharpVersion { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseConsumptionData"/> class.
        /// </summary>
        /// <param name="licensedProduct">The product licensed by the license.</param>
        /// <param name="licenseType">The type of the license.</param>
        /// <param name="licensedFeatures">Features available by the license.</param>
        /// <param name="licensedNamespace">Namespace constraint of the license. <c>null</c> if there is no namespace constraint.</param>
        /// <param name="displayName">The displayable name of the license shown in diagnostics and trace messages.</param>
        public LicenseConsumptionData(
            LicensedProduct licensedProduct,
            LicenseType licenseType,
            LicensedFeatures licensedFeatures,
            string? licensedNamespace,
            string displayName,
            Version minPostSharpVersion )
        {
            this.LicensedProduct = licensedProduct;
            this.LicenseType = licenseType;
            this.LicensedFeatures = licensedFeatures;
            this.LicensedNamespace = licensedNamespace;
            this.DisplayName = displayName;
            this.MinPostSharpVersion = minPostSharpVersion;
        }

        /// <summary>
        /// Gets the displayable name of the license.
        /// </summary>
        /// <returns>The displayable name of the license.</returns>
        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}