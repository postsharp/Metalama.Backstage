// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Configuration;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Telemetry.Metrics;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PostSharp.Backstage.Telemetry
{
    internal class UsageSample : IUsageSample
    {
        private readonly IStandardDirectories _directories;
        private readonly IDateTimeProvider _time;
        private readonly TelemetryConfiguration _configuration;
        private readonly UploadManager _uploadManager;

        public MetricCollection Metrics { get; } = new();

        internal UsageSample( IServiceProvider serviceProvider, TelemetryConfiguration configuration, string eventKind, UploadManager uploadManager )
        {
            this._directories = serviceProvider.GetRequiredService<IStandardDirectories>();
            this._time = serviceProvider.GetRequiredService<IDateTimeProvider>();

            this._configuration = configuration;
            this._uploadManager = uploadManager;

            this.Metrics.Add( new StringMetric( "MetricsEventKind", eventKind ) );

            var process = Process.GetCurrentProcess();

            this.Metrics.Add( new Int32Metric( "Processor.Count", Environment.ProcessorCount ) );

            this.Metrics.Add(
                new StringMetric(
                    "Processor.Architecture",
                    Environment.GetEnvironmentVariable( "PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine ) ) );

            this.Metrics.Add( new StringMetric( "OS.Platform", RuntimeInformation.OSDescription ) );

            this.Metrics.Add(
                new StringMetric(
                    "Net.Architecture",
                    Environment.GetEnvironmentVariable( "PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Process ) ) );

            this.Metrics.Add( new StringMetric( "Net.Version", Environment.Version.ToString() ) );

            var applicationInfo = serviceProvider.GetRequiredService<IApplicationInfo>();
            this.Metrics.Add( new StringMetric( "Application.Build", applicationInfo.Hash ) );
            this.Metrics.Add( new StringMetric( "Application.Version", applicationInfo.Version.ToString() ) );
            this.Metrics.Add( new StringMetric( "Application.ProcessName", process.ProcessName ) );

            this.Metrics.Add( new DateTimeMetric( "Time", this._time.Now ) );
        }

        public void Flush()
        {
            // If no filename was provided, we have to write metrics to the standard reporting directory.
            try
            {
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

                    // Start the upload program once a week.
                    this.Upload();

                    break;
                }
            }
#if DEBUG
            finally { }
#else
                catch
                {
                }
#endif
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

        private void Upload()
        {
            if ( this._configuration.LastUploadTime == null ||
                 this._configuration.LastUploadTime > this._time.Now ||
                 this._configuration.LastUploadTime.Value.AddDays( 1 ) < this._time.Now )
            {
                Task.Run( () => this._uploadManager.Upload() );

                this._configuration.ConfigurationManager.Update<TelemetryConfiguration>( c => c.LastUploadTime = this._time.Now );
            }
        }
    }
}