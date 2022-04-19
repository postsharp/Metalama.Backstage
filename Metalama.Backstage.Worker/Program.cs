// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using System;
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
                    .AddBackstageProcessServices(
                        applicationInfo: new MetalamaBackstageApplicationInfo() );

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
                    var log = services?.GetLoggerFactory()?.Telemetry()?.Error;

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

        private class MetalamaBackstageApplicationInfo : IApplicationInfo
        {
            public string Name => "Metalama.Backstage";

            public string Version
            {
                get
                {
                    var assembly = this.GetType().Assembly;
                    var version = assembly.GetName().Version;

                    if ( version == null )
                    {
                        throw new InvalidOperationException( $"Failed to retrieve the assembly version of '{assembly.FullName}' assembly." );
                    }

                    return version.ToString();
                }
            }

            public bool IsPrerelease => throw new NotImplementedException();

            public DateTime BuildDate => throw new NotImplementedException();

            public ProcessKind ProcessKind => ProcessKind.Backstage;

            public bool IsLongRunningProcess => false;

            public bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => throw new NotImplementedException();
        }
    }
}