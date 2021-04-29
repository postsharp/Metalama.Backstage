// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Backstage.Licensing.Sources
{
    public interface ILicenseSource
    {
        /// <summary>
        /// Gets an enumerable of licenses.
        /// </summary>
        IEnumerable<ILicense> GetLicenses();
    }
}
