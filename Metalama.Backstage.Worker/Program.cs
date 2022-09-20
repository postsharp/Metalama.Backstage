﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
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
        public static async Task Main()
        {
            IServiceProvider? serviceProvider = null;

            var serviceProviderBuilder = new ServiceProviderBuilder()
                .AddMinimalBackstageServices( applicationInfo: new BackstageWorkerApplicationInfo(), addSupportServices: true );

            // Disable logging if the diagnostics.json file has been modified long time ago (i.e. more than X hours before set hours)
            try
            {
                serviceProvider = serviceProviderBuilder.ServiceProvider;
                var configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();

                configurationManager.Update<DiagnosticsConfiguration>( c => c.DisableLoggingForOutdatedSettings() );
            }
            catch ( Exception e )
            {
                if ( !HandleException( serviceProvider, e ) )
                {
                    throw;
                }
            }
            
            // Clean-up is scheduled automatically from Telemetry.
            try
            {
                serviceProvider = serviceProviderBuilder.ServiceProvider;

                var tempFileManager = new TempFileManager( serviceProvider );

                tempFileManager.CleanTempDirectories();
            }
            catch ( Exception e )
            {
                if ( !HandleException( serviceProvider, e ) )
                {
                    throw;
                }
            }

            // Telemetry.
            IUsageSample? usageSample = null;

            try
            {
                usageSample = serviceProviderBuilder.ServiceProvider.GetBackstageService<IUsageReporter>()?.CreateSample( "CompilerUsage" );

                if ( usageSample != null )
                {
                    // ReSharper disable once RedundantTypeArgumentsOfMethod
                    serviceProviderBuilder.AddSingleton<IUsageSample>( usageSample );
                }

                serviceProvider = serviceProviderBuilder.ServiceProvider;

                var uploader = serviceProvider.GetRequiredBackstageService<ITelemetryUploader>();

                await uploader.UploadAsync();
            }
            catch ( Exception e )
            {
                if ( !HandleException( serviceProvider, e ) )
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
                        serviceProvider?.GetBackstageService<IExceptionReporter>()?.ReportException( e );
                    }
                    catch
                    {
                        // We don't want failing telemetry to disturb users.
                    }

                    // We don't re-throw here as we don't want compiler to crash because of usage reporting exceptions.
                }
            }
        }

        public static bool HandleException( IServiceProvider? serviceProvider, Exception e )
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

                    return true;
                }
            }
            catch
            {
                // We don't want failing telemetry to disturb users.
            }

            return false;
        }
    }
}