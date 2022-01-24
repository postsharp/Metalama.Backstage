// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Licensing.Consumption.Sources
{
    /// <summary>
    /// Source of licenses for consumption.
    /// </summary>
    public interface ILicenseSource
    {
        /// <summary>
        /// Gets an enumerable of licenses.
        /// </summary>
        /// <returns>The enumerable of licenses.</returns>
        IEnumerable<ILicense> GetLicenses( Action<LicensingMessage> reportMessage );
    }
}