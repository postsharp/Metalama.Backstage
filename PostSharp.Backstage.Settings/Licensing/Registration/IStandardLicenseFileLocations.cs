// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Provides standard paths of license files.
    /// </summary>
    public interface IStandardLicenseFileLocations
    {
        /// <summary>
        /// Gets the path to the license file containing user-wise registered licenses.
        /// </summary>
        string UserLicenseFile { get; }
    }
}