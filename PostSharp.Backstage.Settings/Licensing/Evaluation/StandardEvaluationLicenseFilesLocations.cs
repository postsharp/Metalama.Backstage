// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;
using PostSharp.Backstage.Settings;

namespace PostSharp.Backstage.Licensing.Evaluation
{
    internal static class StandardEvaluationLicenseFilesLocations
    {
        public static readonly string EvaluationLicenseFile = Path.Combine( StandardDirectories.ApplicationDataDirectory, "licenseregistration.cnf" );
    }
}
