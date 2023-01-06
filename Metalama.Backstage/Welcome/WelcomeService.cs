// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing;
using Metalama.Backstage.Licensing.Registration.Evaluation;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Utilities;
using System;
using System.Diagnostics;

namespace Metalama.Backstage.Welcome;

public class WelcomeService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly IConfigurationManager _configurationManager;
    private readonly WelcomeConfiguration _welcomeConfiguration;

    public WelcomeService( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Welcome" );
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._welcomeConfiguration = this._configurationManager.Get<WelcomeConfiguration>();
    }

    private void ExecuteOnce(
        Action action,
        string actionName,
        Func<WelcomeConfiguration, bool> getIsFirst,
        Func<WelcomeConfiguration, WelcomeConfiguration> setIsNotFirst )
    {
        if ( !getIsFirst( this._welcomeConfiguration ) )
        {
            this._logger.Trace?.Log( $"The {actionName} action has already been executed." );

            return;
        }

        // We need a global lock to start the welcome service because several process may attempt to start the evaluation
        // license. We need the processes who lost the race to wait, so they can read the configuration file.

        using ( MutexHelper.WithGlobalLock( $"Welcome{actionName}" ) )
        {
            if ( !this._configurationManager.UpdateIf(
                    c => !getIsFirst( c ),
                    setIsNotFirst ) )
            {
                // Another process has won the race.

                this._logger.Trace?.Log( $"{actionName} action has been executed by a concurrent process." );

                return;
            }

            action();
        }
    }

    public void ExecuteFirstStartSetup( bool registerEvaluationLicense = true )
    {
        this.ExecuteOnce(
            () =>
            {
                // Start the evaluation license.
                if ( registerEvaluationLicense )
                {
                    var evaluationLicenseRegistrar = new EvaluationLicenseRegistrar( this._serviceProvider );
                    evaluationLicenseRegistrar.TryActivateLicense();
                }

                // Activate telemetry.
                if ( !TelemetryConfiguration.IsOptOutEnvironmentVariableSet() )
                {
                    this._logger.Trace?.Log( "Enabling telemetry." );

                    this._configurationManager.Update<TelemetryConfiguration>(
                        c => c with
                        {
                            // Enable telemetry except if it has been disabled by the command line.
                            ExceptionReportingAction = c.ExceptionReportingAction == ReportingAction.Ask ? ReportingAction.Yes : c.ExceptionReportingAction,
                            PerformanceProblemReportingAction =
                            c.PerformanceProblemReportingAction == ReportingAction.Yes ? ReportingAction.Yes : c.PerformanceProblemReportingAction,
                            UsageReportingAction = c.UsageReportingAction == ReportingAction.Ask ? ReportingAction.Yes : c.UsageReportingAction
                        } );
                }
            },
            nameof(this.ExecuteFirstStartSetup),
            c => c.IsFirstStart,
            c => c with { IsFirstStart = false } );
    }

    public void OpenWelcomePage()
    {
        this.ExecuteOnce(
            () =>
            {
                try
                {
                    var applicationInfo = this._serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;

                    var url = applicationInfo.IsPreviewLicenseEligible()
                        ? "https://www.postsharp.net/links/metalama-welcome-preview"
                        : "https://www.postsharp.net/links/metalama-welcome";

                    this._logger.Trace?.Log( $"Opening '{url}'." );

                    Process.Start( new ProcessStartInfo( url ) { UseShellExecute = true } );
                }
                catch ( Exception e )
                {
                    this._logger.Error?.Log( $"Cannot start the welcome web page: {e.Message}" );
                }
            },
            nameof(this.OpenWelcomePage),
            c => c.IsWelcomePagePending,
            c => c with { IsWelcomePagePending = false } );
    }
}