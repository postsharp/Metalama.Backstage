// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Utilities;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;

namespace Metalama.Backstage
{
    internal static class Program
    {
        public static async Task Main()
        {
            IServiceProvider? services = null;

            try
            {
                var serviceProviderBuilder = new ServiceProviderBuilder()
                    .AddMinimalBackstageServices( applicationInfo: new ApplicationInfo(), addSupportServices: true );

                services = serviceProviderBuilder.ServiceProvider;

                var uploader = new TelemetryUploader( services );

                await uploader.UploadAsync();
            }
            catch ( Exception e )
            {
                var isReported = false;

                try
                {
                    var exceptionReporter = services?.GetService<IExceptionReporter>();

                    if ( exceptionReporter != null )
                    {
                        exceptionReporter.ReportException( e );
                        isReported = true;
                    }
                }
                catch
                {
                    // We don't want failing telemetry to disturb users.
                }

                try
                {
                    var log = services?.GetLoggerFactory().Telemetry().Error;

                    if ( log != null )
                    {
                        log.Log( $"Unhandled exception: {e}" );
                        isReported = true;
                    }
                }
                catch
                {
                    // We don't want failing telemetry to disturb users.
                }

                if ( !isReported )
                {
                    throw;
                }
            }
        }

        private class ApplicationInfo : IApplicationInfo
        {
            public ApplicationInfo()
            {
                var metadataAttributes =
                    typeof(ApplicationInfo).Assembly.GetCustomAttributes(
                        typeof(AssemblyMetadataAttribute),
                        inherit: false );

                string? version = null;
                bool? isPrerelease = null;
                DateTime? buildDate = null;

                bool AllMetadataFound() => version != null && isPrerelease != null && buildDate != null;

                foreach ( var metadataAttributeObject in metadataAttributes )
                {
                    var metadataAttribute = (AssemblyMetadataAttribute) metadataAttributeObject;

                    switch ( metadataAttribute.Key )
                    {
                        case "PackageVersion":
                            if ( !string.IsNullOrEmpty( metadataAttribute.Value ) )
                            {
                                version = metadataAttribute.Value;
                                isPrerelease = version.Contains( "-" );
                            }

                            break;

                        case "PackageBuildDate":
                            if ( !string.IsNullOrEmpty( metadataAttribute.Value ) )
                            {
                                buildDate = DateTime.Parse( metadataAttribute.Value, CultureInfo.InvariantCulture );
                            }

                            break;
                    }

                    if ( AllMetadataFound() )
                    {
                        break;
                    }
                }

                if ( !AllMetadataFound() )
                {
                    throw new InvalidOperationException( $"{nameof(ApplicationInfo)} has failed to find some of the required assembly metadata." );
                }

                this.Version = version!;
                this.IsPrerelease = isPrerelease!.Value;
                this.BuildDate = buildDate!.Value;
            }
            
            public string Name => "Metalama Backstage Worker";

            public string Version { get; }

            public bool IsPrerelease { get; }

            public DateTime BuildDate { get; }

            public ProcessKind ProcessKind => ProcessKind.BackstageWorker;

            public bool IsLongRunningProcess => false;

            public bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => ProcessUtilities.IsCurrentProcessUnattended( loggerFactory );
        }
    }
}