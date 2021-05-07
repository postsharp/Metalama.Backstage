// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;
using PostSharp.Backstage.Settings;

namespace PostSharp.Backstage.Licensing.Registration
{
    public static class StandardLicenseFilesLocations
    {
        public static readonly string UserLicenseFile = Path.Combine( StandardSettingsFileLocation.Path, "postsharp.lic" );
    }
}
