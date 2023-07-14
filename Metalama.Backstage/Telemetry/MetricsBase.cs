// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;
using System.IO;
using System.Text;

namespace Metalama.Backstage.Telemetry;

internal abstract class MetricsBase
{
    private readonly IStandardDirectories _directories;
    private readonly IFileSystem _fileSystem;
    private readonly ITelemetryUploader _uploader;
    private readonly ILogger _logger;
    private readonly string _feedbackKind;

    protected MetricCollection Metrics { get; } = new();

    protected MetricsBase( IServiceProvider serviceProvider, string feedbackKind )
    {
        this._directories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
        this._uploader = serviceProvider.GetRequiredBackstageService<ITelemetryUploader>();
        this._logger = serviceProvider.GetRequiredBackstageService<ILoggerFactory>().Telemetry();
        this._feedbackKind = feedbackKind;
    }

    public void Flush()
    {
        // If no filename was provided, we have to write metrics to the standard reporting directory.

        this.CreateUploadDirectory();

        // We try 8 different files to avoid locks.
        for ( var i = 0; i < 8; i++ )
        {
            try
            {
                var fileName = Path.Combine( this._directories.TelemetryUploadQueueDirectory, $"{this._feedbackKind}-{i}.log" );
                this.Flush( fileName );
            }
            catch
            {
                continue;
            }

            // Start the upload periodically.
            this._uploader.StartUpload();

            break;
        }
    }

    private void Flush( string fileName )
    {
        this._logger.Trace?.Log( $"Flushing usage to '{fileName}' file." );

        var directory = Path.GetDirectoryName( fileName );

        if ( directory != null )
        {
            this._fileSystem.CreateDirectory( directory );
        }

        using ( var stream = this._fileSystem.Open( fileName, FileMode.Append, FileAccess.Write, FileShare.None ) )
        using ( var writer = new StreamWriter( stream, Encoding.UTF8 ) )
        {
            this.Metrics.Write( writer );
            writer.Write( Environment.NewLine );
        }

        this._logger.Trace?.Log( $"Usage written to '{fileName}' file." );
    }

    private void CreateUploadDirectory()
    {
        if ( !this._fileSystem.DirectoryExists( this._directories.TelemetryUploadQueueDirectory ) )
        {
            this._logger.Trace?.Log( $"Creating '{this._directories.TelemetryUploadQueueDirectory}' directory." );
            this._fileSystem.CreateDirectory( this._directories.TelemetryUploadQueueDirectory );
        }
    }
}