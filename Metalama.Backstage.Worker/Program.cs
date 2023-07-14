// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Telemetry;
using System;
using System.Threading.Tasks;

namespace Metalama.Backstage
{
    internal static class Program
    {
        private static bool _canIgnoreRecoverableExceptions = true;

        public static async Task Main()
        {
            var initializationOptions = new BackstageInitializationOptions( new BackstageWorkerApplicationInfo() ) { AddSupportServices = true };
            var serviceProviderBuilder = new ServiceProviderBuilder().AddBackstageServices( initializationOptions );
            var serviceProvider = serviceProviderBuilder.ServiceProvider;

            try
            {
                _canIgnoreRecoverableExceptions = serviceProvider.GetRequiredBackstageService<IRecoverableExceptionService>().CanIgnore;
                var logger = serviceProvider.GetLoggerFactory().GetLogger( "Worker" );
                var usageReporter = serviceProviderBuilder.ServiceProvider.GetBackstageService<IUsageReporter>();

                try
                {
                    logger.Trace?.Log( "Job started." );
                    usageReporter?.StartSession( "CompilerUsage" );

                    // Clean-up. Scheduled automatically by telemetry.
                    logger.Trace?.Log( "Starting temporary directories cleanup." );
                    var tempFileManager = new TempFileManager( serviceProvider );
                    tempFileManager.CleanTempDirectories();

                    // Telemetry.
                    logger.Trace?.Log( "Starting telemetry upload." );
                    var uploader = serviceProvider.GetRequiredBackstageService<ITelemetryUploader>();
                    await uploader.UploadAsync();

                    logger.Trace?.Log( "Job done." );
                }
                finally
                {
                    // Report usage.
                    usageReporter?.StopSession();
                }
            }
            catch ( Exception e )
            {
                if ( !HandleException( serviceProvider, e ) )
                {
                    throw;
                }

                if ( !_canIgnoreRecoverableExceptions )
                {
                    throw;
                }
            }
            finally
            {
                try
                {
                    // Close logs.
                    // Logging has to be disposed as the last one, so it could be used until now.
                    serviceProvider?.GetLoggerFactory().Dispose();
                }
                catch ( Exception e )
                {
                    try
                    {
                        serviceProvider?.GetBackstageService<IExceptionReporter>()?.ReportException( e );
                    }
                    catch when ( _canIgnoreRecoverableExceptions )
                    {
                        // We don't want failing telemetry to disturb users.
                    }

                    // We don't re-throw here as we don't want compiler to crash because of usage reporting exceptions.
                    if ( !_canIgnoreRecoverableExceptions )
                    {
                        throw;
                    }
                }
            }
        }

        private static bool HandleException( IServiceProvider? serviceProvider, Exception e )
        {
            try
            {
                var exceptionReporter = serviceProvider?.GetBackstageService<IExceptionReporter>();

                if ( exceptionReporter != null )
                {
                    exceptionReporter.ReportException( e );

                    return true;
                }
            }
            catch when ( _canIgnoreRecoverableExceptions )
            {
                // We don't want failing telemetry to disturb users.
            }

            try
            {
                var log = serviceProvider?.GetLoggerFactory().Telemetry().Error;

                if ( log != null )
                {
                    log.Log( $"Unhandled exception: {e}" );

                    return true;
                }
            }
            catch when ( _canIgnoreRecoverableExceptions )
            {
                // We don't want failing telemetry to disturb users.
            }

            return false;
        }
    }
}