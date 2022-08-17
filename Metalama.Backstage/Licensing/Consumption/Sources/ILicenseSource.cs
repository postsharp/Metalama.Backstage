﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Licensing.Licenses;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Licensing.Consumption.Sources
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

        /// <summary>
        /// Gets a value indicating whether the license source can provide redistribution licenses to be embedded in an output assembly.
        /// </summary>
        bool IsRedistributionLicenseSource { get; }
    }
}