// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using System;
using System.Threading.Tasks;

namespace Metalama.Backstage
{
    internal class Program
    {
        public static async Task Main(string[] args)
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
                catch { }

                try
                {
                    var log = services?.GetLoggerFactory()?.Telemetry()?.Error;

                    if ( log != null )
                    {
                        log.Log( $"Unhandled exception: {e}" );
                        isReported = true;
                    }
                }
                catch { }

                if ( !isReported )
                {
                    throw;
                }
            }
        }

        private class MetalamaBackstageApplicationInfo : IApplicationInfo
        {
            public string Name => "Metalama.Backstage";

            public string Version => this.GetType().Assembly.GetName().Version.ToString();

            public bool IsPrerelease => throw new System.NotImplementedException();

            public System.DateTime BuildDate => throw new System.NotImplementedException();

            public ProcessKind ProcessKind => ProcessKind.Backstage;

            public bool IsLongRunningProcess => false;

            public bool IsUnattendedProcess( ILoggerFactory loggerFactory )
            {
                throw new System.NotImplementedException();
            }
        }
    }
}