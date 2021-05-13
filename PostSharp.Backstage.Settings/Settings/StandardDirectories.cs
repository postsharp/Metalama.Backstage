// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace PostSharp.Backstage.Settings
{
    // https://enbravikov.wordpress.com/2018/09/15/special-folder-enum-values-on-windows-and-linux-ubuntu-16-04-in-net-core/

    /// <summary>
    /// Paths of standard directories.
    /// </summary>
    public static class StandardDirectories
    {
        /// <summary>
        /// The directory that serves as a common repository for application-specific data for the current roaming user.
        /// </summary>
        public static readonly string ApplicationDataDirectory = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), ".postsharp" );

        /// <summary>
        /// The path of the current user's application temporary folder.
        /// </summary>
        public static readonly string TempDirectory = Path.Combine( Path.GetTempPath(), "PostSharp" );
    }
}
