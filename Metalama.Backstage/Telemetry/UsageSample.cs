// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry.Metrics;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Metalama.Backstage.Telemetry
{
    internal class UsageSample : IUsageSample
    {
        private readonly IStandardDirectories _directories;
        private readonly TelemetryUploader _uploader;

        public MetricCollection Metrics { get; } = new();

        internal UsageSample( IServiceProvider serviceProvider, string eventKind, TelemetryUploader uploader )
        {
            this._directories = serviceProvider.GetRequiredService<IStandardDirectories>();
            var time = serviceProvider.GetRequiredService<IDateTimeProvider>();
            var applicationInfo = serviceProvider.GetRequiredService<IApplicationInfoProvider>().CurrentApplication;
            var loggerFactory = serviceProvider.GetLoggerFactory();

            this._uploader = uploader;

            this.Metrics.Add( new StringMetric( "MetricsEventKind", eventKind ) );

            this.Metrics.Add( new Int32Metric( "Processor.Count", Environment.ProcessorCount ) );
            this.Metrics.Add( new StringMetric( "Processor.Architecture", RuntimeInformation.ProcessArchitecture.ToString() ) );

            this.Metrics.Add( new StringMetric( "OS.Platform", RuntimeInformation.OSDescription ) );

            this.Metrics.Add( new StringMetric( "Net.Architecture", RuntimeInformation.ProcessArchitecture.ToString() ) );
            this.Metrics.Add( new StringMetric( "Net.Version", Environment.Version.ToString() ) );

            this.Metrics.Add( new StringMetric( "Application.Name", applicationInfo.Name ) );
            this.Metrics.Add( new StringMetric( "Application.Version", applicationInfo.Version ) );
            this.Metrics.Add( new BoolMetric( "Application.IsUnattended", applicationInfo.IsUnattendedProcess( loggerFactory ) ) );
            this.Metrics.Add( new StringMetric( "Application.ProcessName", Process.GetCurrentProcess().ProcessName ) );
            this.Metrics.Add( new StringMetric( "Application.ProcessKind", applicationInfo.ProcessKind.ToString() ) );
            this.Metrics.Add( new StringMetric( "Application.EntryAssembly", Path.GetFileName( Assembly.GetEntryAssembly()?.Location ) ) );

            this.Metrics.Add( new DateTimeMetric( "Time", time.Now ) );
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
                    var fileName = Path.Combine(
                        this._directories.TelemetryUploadQueueDirectory,
                        string.Format( CultureInfo.InvariantCulture, "Usage-{0}.log", i ) );

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
}