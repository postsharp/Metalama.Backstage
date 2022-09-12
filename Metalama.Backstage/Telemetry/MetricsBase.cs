// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;
using System.IO;
using System.Text;

namespace Metalama.Backstage.Telemetry;

internal abstract class MetricsBase
{
    private readonly IStandardDirectories _directories;
    private readonly ITelemetryUploader _uploader;
    private readonly string _feedbackKind;

    protected MetricCollection Metrics { get; } = new();

    protected MetricsBase( IServiceProvider serviceProvider, string feedbackKind )
    {
        this._directories = serviceProvider.GetRequiredBackstageService<IStandardDirectories>();
        this._uploader = serviceProvider.GetRequiredBackstageService<ITelemetryUploader>();
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
        var directory = Path.GetDirectoryName( fileName );

        if ( directory != null )
        {
            Directory.CreateDirectory( directory );
        }

        using ( Stream stream = new FileStream( fileName, FileMode.Append, FileAccess.Write, FileShare.None ) )
        using ( var writer = new StreamWriter( stream, Encoding.UTF8 ) )
        {
            this.Metrics.Write( writer );
            writer.Write( Environment.NewLine );
        }
    }

    private void CreateUploadDirectory()
    {
        if ( !Directory.Exists( this._directories.TelemetryUploadQueueDirectory ) )
        {
            Directory.CreateDirectory( this._directories.TelemetryUploadQueueDirectory );
        }
    }
}