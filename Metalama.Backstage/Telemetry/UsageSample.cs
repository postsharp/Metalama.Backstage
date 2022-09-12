// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry.Metrics;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Metalama.Backstage.Telemetry
{
    internal class UsageSample : MetricsBase, IUsageSample
    {
        MetricCollection IUsageSample.Metrics => this.Metrics;

        internal UsageSample( IServiceProvider serviceProvider, string eventKind )
            : base( serviceProvider, "Usage" )
        {
            var time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
            var applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
            var loggerFactory = serviceProvider.GetLoggerFactory();
            
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
    }
}