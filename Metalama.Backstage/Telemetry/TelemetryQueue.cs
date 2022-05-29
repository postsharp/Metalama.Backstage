// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;
using System.IO;
using System.Text;

namespace Metalama.Backstage.Telemetry
{
    internal sealed class TelemetryQueue
    {
        private readonly IStandardDirectories _directories;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;

        public TelemetryQueue( IServiceProvider serviceProvider )
        {
            this._directories = serviceProvider.GetRequiredService<IStandardDirectories>();
            this._fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
            this._logger = serviceProvider.GetLoggerFactory().Telemetry();
        }

        public void EnqueueFile( string file )
        {
            this._logger.Trace?.Log( $"Enqueuing '{file}'." );

            var directory = this._directories.TelemetryUploadQueueDirectory;

            this._fileSystem.CreateDirectory( directory );
            this._fileSystem.MoveFile( file, Path.Combine( directory, Path.GetFileName( file ) ) );
        }

        public void EnqueueContent( string fileName, string contents )
        {
            this._logger.Trace?.Log( $"Enqueuing content of '{fileName}'." );

            var tempFile = Path.GetTempFileName();

            this._fileSystem.WriteAllText( tempFile, contents, Encoding.UTF8 );

            var directory = this._directories.TelemetryUploadQueueDirectory;

            this._fileSystem.CreateDirectory( directory );
            this._fileSystem.MoveFile( tempFile, Path.Combine( this._directories.TelemetryUploadQueueDirectory, fileName ) );
        }
    }
}