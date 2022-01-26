// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Metalama.Backstage.Extensibility
{
    // https://enbravikov.wordpress.com/2018/09/15/special-folder-enum-values-on-windows-and-linux-ubuntu-16-04-in-net-core/

    /// <inheritdoc />
    internal class StandardDirectories : IStandardDirectories
    {
        /// <inheritdoc />
        public string ApplicationDataDirectory { get; } =
            Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), ".metalama" );

        /// <inheritdoc />
        public string TempDirectory { get; } = Path.Combine( Path.GetTempPath(), "PostSharp" );

        public string TelemetryExceptionsDirectory => Path.Combine( this.ApplicationDataDirectory, "Telemetry", "Exceptions" );

        public string TelemetryUploadQueueDirectory => Path.Combine( this.ApplicationDataDirectory, "Telemetry", "UploadQueue" );

        public string TelemetryUploadPackagesDirectory => Path.Combine( this.ApplicationDataDirectory, "Telemetry", "Packages" );
    }
}