// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Settings
{
    /// <summary>
    /// Standard location of settings files.
    /// </summary>
    public static class StandardSettingsFilesLocation
    {
        // https://enbravikov.wordpress.com/2018/09/15/special-folder-enum-values-on-windows-and-linux-ubuntu-16-04-in-net-core/
        public static readonly string Path = System.IO.Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), ".postsharp" );
    }
}
