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
            IServiceProvider? serviceProvider = null;
            IUsageSample? usageSample = null;

            try
            {
                var serviceProviderBuilder = new ServiceProviderBuilder()
                    .AddMinimalBackstageServices( applicationInfo: new BackstageWorkerApplicationInfo(), addSupportServices: true );

                usageSample = serviceProviderBuilder.ServiceProvider.GetService<IUsageReporter>()?.CreateSample( "CompilerUsage" );

                if ( usageSample != null )
                {
                    // ReSharper disable once RedundantTypeArgumentsOfMethod
                    serviceProviderBuilder.AddSingleton<IUsageSample>( usageSample );
                }

                serviceProvider = serviceProviderBuilder.ServiceProvider;

                var uploader = new TelemetryUploader( serviceProvider );

                await uploader.UploadAsync();
            }
            catch ( Exception e )
            {
                var isReported = false;

                try
                {
                    var exceptionReporter = serviceProvider?.GetService<IExceptionReporter>();

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
                    var log = serviceProvider?.GetLoggerFactory().Telemetry().Error;

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
            finally
            {
                try
                {
                    // Report usage.
                    usageSample?.Flush();

                    // Close logs.
                    // Logging has to be disposed as the last one, so it could be used until now.
                    serviceProvider?.GetLoggerFactory().Dispose();
                }
                catch ( Exception e )
                {
                    try
                    {
                        serviceProvider?.GetService<IExceptionReporter>()?.ReportException( e );
                    }
                    catch
                    {
                        // We don't want failing telemetry to disturb users.
                    }

                    // We don't re-throw here as we don't want compiler to crash because of usage reporting exceptions.
                }
            }
        }
    }
}