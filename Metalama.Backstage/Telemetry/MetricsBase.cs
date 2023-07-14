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
        this._feedbackKind = feedbackKind;

        this._directories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this._fileSystem = serviceProvider.GetRequiredBackstageService<IFileSystem>();
        this._uploader = serviceProvider.GetRequiredBackstageService<ITelemetryUploader>();
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Metrics" );
    }

    public void Upload()
    {
        // If no filename was provided, we have to write metrics to the standard reporting directory.

        this.CreateUploadDirectory();

        try
        {
            var fileName = Path.Combine( this._directories.TelemetryUploadQueueDirectory, $"{this._feedbackKind}-{Guid.NewGuid()}.log" );
            this.Write( fileName );
        }
        catch ( Exception e )
        {
            this._logger.Error?.Log( e.ToString() );

            return;
        }

        // Start the upload periodically.
        this._uploader.StartUpload();
    }

    private void Write( string fileName )
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