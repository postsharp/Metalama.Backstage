// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Backstage.Licensing.Sources
{
    public interface ILicenseSource
    {
        /// <summary>
        /// Gets license strings.
        /// </summary>
        /// <remarks>
        /// Each license string can represent either a license key or a license server URL.
        /// </remarks>
        IEnumerable<ILicense> Licenses { get; }
    }
}
