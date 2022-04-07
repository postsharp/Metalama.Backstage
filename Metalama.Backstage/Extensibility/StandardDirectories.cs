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
        public StandardDirectories()
        {
            if ( string.IsNullOrEmpty( this.ApplicationDataDirectory ) )
            {
                throw new InvalidOperationException( "Failed to initialize standard directories." );
            }
        }
        
        /// <inheritdoc />
        public string ApplicationDataDirectory { get; } =
            Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), ".metalama" );

        /// <inheritdoc />
        public string TempDirectory { get; } = Path.Combine( Path.GetTempPath(), "Metalama" );

        /// <inheritdoc />
        public string TelemetryExceptionsDirectory => Path.Combine( this.ApplicationDataDirectory, "Telemetry", "Exceptions" );

        /// <inheritdoc />
        public string TelemetryUploadQueueDirectory => Path.Combine( this.ApplicationDataDirectory, "Telemetry", "UploadQueue" );

        /// <inheritdoc />
        public string TelemetryUploadPackagesDirectory => Path.Combine( this.ApplicationDataDirectory, "Telemetry", "Packages" );
    }
}