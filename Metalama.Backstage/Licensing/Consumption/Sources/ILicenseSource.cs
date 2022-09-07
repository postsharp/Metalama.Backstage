// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Licenses;
using System;

namespace Metalama.Backstage.Licensing.Consumption.Sources
{
    /// <summary>
    /// Source of licenses for consumption.
    /// </summary>
    public interface ILicenseSource
    {
        /// <summary>
        /// Gets a license, if available and valid. <paramref name="reportMessage"/> is called when the license key is invalid.
        /// </summary>
        /// <param name="reportMessage">Action to be called when the message is invalid.</param>
        /// <returns>The license or <c>null</c>.</returns>
        ILicense? GetLicense( Action<LicensingMessage> reportMessage );
    }
}