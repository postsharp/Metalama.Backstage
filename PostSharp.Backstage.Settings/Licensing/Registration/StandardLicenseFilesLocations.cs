// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;
using PostSharp.Backstage.Settings;

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Standard paths of license files.
    /// </summary>
    public static class StandardLicenseFilesLocations
    {
        /// <summary>
        /// Path to the license file containing user-wise registered licenses.
        /// </summary>
        public static readonly string UserLicenseFile = Path.Combine( StandardDirectories.ApplicationDataDirectory, "postsharp.lic" );
    }
}
