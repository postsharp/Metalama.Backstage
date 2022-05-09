// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Registration.Evaluation;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Utilities;
using System;
using System.Diagnostics;

namespace Metalama.Backstage.Welcome;

public static class WelcomeService
{
    public static void Execute( IServiceProvider serviceProvider, bool registerEvaluationLicense = true )
    {
        var logger = serviceProvider.GetLoggerFactory().GetLogger( "Welcome" );
        var configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();
        var welcomeConfiguration = configurationManager.Get<WelcomeConfiguration>();
        var applicationInfo = serviceProvider.GetRequiredService<IApplicationInfo>();

        if ( !welcomeConfiguration.IsFirstStart )
        {
            logger.Trace?.Log( "The Welcome service has already been executed." );

            return;
        }

        // We need a global lock to start the welcome service because several process may attempt to start the evaluation
        // license. We need the processes who lost the race to wait, so they can read the configuration file.

        using ( MutexHelper.WithGlobalLock( "Welcome" ) )
        {
            if ( !configurationManager.Update<WelcomeConfiguration>(
                    c =>
                    {
                        if ( c.IsFirstStart )
                        {
                            c.IsFirstStart = false;

                            return true;
                        }
                        else
                        {
                            // Another process has won the race.

                            return false;
                        }
                    } ) )
            {
                // Another process has won the race.

                logger.Trace?.Log( "Welcome service has been executed by a concurrent process." );

                return;
            }

            // Start the evaluation license.
            if ( registerEvaluationLicense )
            {
                var evaluationLicenseRegistrar = new EvaluationLicenseRegistrar( serviceProvider );
                evaluationLicenseRegistrar.TryActivateLicense();
            }

            // Activate telemetry.
            if ( !TelemetryConfiguration.IsOptOutEnvironmentVariableSet() )
            {
                logger.Trace?.Log( "Enabling telemetry." );

                configurationManager.Update<TelemetryConfiguration>(
                    c =>
                    {
                        // Enable telemetry except if it has been disabled by the command line.

                        if ( c.ExceptionReportingAction == ReportingAction.Ask )
                        {
                            c.ExceptionReportingAction = ReportingAction.Yes;
                        }

                        if ( c.PerformanceProblemReportingAction == ReportingAction.Ask )
                        {
                            c.PerformanceProblemReportingAction = ReportingAction.Yes;
                        }

                        if ( c.ReportUsage == ReportingAction.Ask )
                        {
                            c.ReportUsage = ReportingAction.Yes;
                        }
                    } );
            }

            // Open the welcome web page.
            try
            {
                var url = applicationInfo.IsPrerelease
                    ? "https://www.postsharp.net/links/metalama-welcome-preview"
                    : "https://www.postsharp.net/links/metalama-welcome";

                logger.Trace?.Log( $"Opening '{url}'." );

                Process.Start( new ProcessStartInfo( url ) { UseShellExecute = true } );
            }
            catch ( Exception e )
            {
                logger.Error?.Log( $"Cannot start the welcome web page: {e.Message}" );
            }
        }
    }
}