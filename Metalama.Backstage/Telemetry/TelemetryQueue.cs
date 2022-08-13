// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

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
        private readonly ILogger _logger;

        public TelemetryQueue( IServiceProvider serviceProvider )
        {
            this._directories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
            this._logger = serviceProvider.GetLoggerFactory().Telemetry();
        }

        public void EnqueueFile( string file )
        {
            this._logger.Trace?.Log( $"Enqueuing '{file}'." );

            var directory = this._directories.TelemetryUploadQueueDirectory;

            if ( !Directory.Exists( directory ) )
            {
                Directory.CreateDirectory( directory );
            }

            File.Move( file, Path.Combine( directory, Path.GetFileName( file ) ) );
        }

        public void EnqueueContent( string fileName, string contents )
        {
            this._logger.Trace?.Log( $"Enqueuing content of '{fileName}'." );

            var tempFile = Path.GetTempFileName();

            File.WriteAllText( tempFile, contents, Encoding.UTF8 );

            if ( !Directory.Exists( this._directories.TelemetryUploadQueueDirectory ) )
            {
                Directory.CreateDirectory( this._directories.TelemetryUploadQueueDirectory );
            }

            File.Move( tempFile, Path.Combine( this._directories.TelemetryUploadQueueDirectory, fileName ) );
        }
    }
}