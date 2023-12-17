// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using System;
using System.IO;

namespace Metalama.Backstage.Infrastructure
{
    // We base this class on
    // https://enbravikov.wordpress.com/2018/09/15/special-folder-enum-values-on-windows-and-linux-ubuntu-16-04-in-net-core/
    // but not all platforms respect this. For such platforms, we provide fallbacks.

    /// <inheritdoc />
    internal class StandardDirectories : IStandardDirectories
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDirectories"/> class.
        /// </summary>
        public StandardDirectories()
        {
            var applicationDataParentDirectory = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );

            if ( string.IsNullOrEmpty( applicationDataParentDirectory ) )
            {
                // This is a fallback for Ubuntu on WSL and other platforms that don't provide
                // the SpecialFolder.ApplicationData folder path.
                applicationDataParentDirectory = Environment.GetFolderPath( Environment.SpecialFolder.UserProfile );
            }

            if ( string.IsNullOrEmpty( applicationDataParentDirectory ) )
            {
                // This will always fail on platforms which don't provide the special folders being discovered above.
                // We need to find another locations on such platforms.
                throw new InvalidOperationException( "Failed to find application data parent directory." );
            }

            this.ApplicationDataDirectory = Path.Combine( applicationDataParentDirectory, ".metalama" );
        }

        /// <inheritdoc />
        public string ApplicationDataDirectory { get; }

        /// <inheritdoc />
        public string TempDirectory { get; } = Path.Combine( MetalamaPathUtilities.GetTempPath(), "Metalama" );

        /// <inheritdoc />
        public string TelemetryExceptionsDirectory => Path.Combine( this.ApplicationDataDirectory, "Telemetry", "Exceptions" );

        /// <inheritdoc />
        public string TelemetryUploadQueueDirectory => Path.Combine( this.ApplicationDataDirectory, "Telemetry", "UploadQueue" );

        /// <inheritdoc />
        public string TelemetryUploadPackagesDirectory => Path.Combine( this.ApplicationDataDirectory, "Telemetry", "Packages" );
    }
}