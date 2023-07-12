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
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private readonly IConfigurationManager _configurationManager;
    private readonly WelcomeConfiguration _welcomeConfiguration;
    private readonly IProcessExecutor _processExecutor;

    public WelcomeService( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._loggerFactory = serviceProvider.GetLoggerFactory(); 
        this._logger = this._loggerFactory.GetLogger( "Welcome" );
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._welcomeConfiguration = this._configurationManager.Get<WelcomeConfiguration>();
        this._processExecutor = serviceProvider.GetRequiredBackstageService<IProcessExecutor>();
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
                    c => getIsFirst( c ),
                    setIsNotFirst ) )
            {
                // Another process has won the race.

                this._logger.Trace?.Log( $"{actionName} action has been executed by a concurrent process." );

                return;
            }

            action();
        }
    }

    public void ExecuteFirstStartSetup( BackstageInitializationOptions options )
    {
        var ignoreUserProfileLicenses = options.LicensingOptions.IgnoreUserProfileLicenses;
        var isPreviewLicenseEligible = options.ApplicationInfo.IsPreviewLicenseEligible();
        var isUnattendedProcess = options.ApplicationInfo.IsUnattendedProcess( this._loggerFactory );

        var registerEvaluationLicense = !ignoreUserProfileLicenses
                                        && !isPreviewLicenseEligible
                                        && !isUnattendedProcess;

        this._logger.Trace?.Log( $"{nameof(ignoreUserProfileLicenses)}: {ignoreUserProfileLicenses}" );
        this._logger.Trace?.Log( $"{nameof(isPreviewLicenseEligible)}: {isPreviewLicenseEligible}" );
        this._logger.Trace?.Log( $"{nameof(isUnattendedProcess)}: {isUnattendedProcess}" );
        this._logger.Trace?.Log( $"{nameof(registerEvaluationLicense)}: {registerEvaluationLicense}" );

        this.ExecuteFirstStartSetup( registerEvaluationLicense, options.OpenWelcomePage );
    }

    public void ExecuteFirstStartSetup( bool registerEvaluationLicense = true, bool openWelcomePage = true )
    {
        if ( registerEvaluationLicense )
        {
            this.ExecuteOnce(
                this.RegisterEvaluationLicense,
                nameof(this.RegisterEvaluationLicense),
                c => c.IsFirstTimeEvaluationLicenseRegistrationPending,
                c => c with { IsFirstTimeEvaluationLicenseRegistrationPending = false } );
        }

        this.ExecuteOnce(
            this.ActivateTelemetry,
            nameof(this.ActivateTelemetry),
            c => c.IsFirstStart,
            c => c with { IsFirstStart = false } );

        if ( openWelcomePage )
        {
            this.ExecuteOnce(
                this.OpenWelcomePage,
                nameof(this.OpenWelcomePage),
                c => c.IsWelcomePagePending,
                c => c with { IsWelcomePagePending = false } );
        }
    }

    private void RegisterEvaluationLicense()
    {
        var evaluationLicenseRegistrar = new EvaluationLicenseRegistrar( this._serviceProvider );
        evaluationLicenseRegistrar.TryActivateLicense();
    }

    private void ActivateTelemetry()
    {
        if ( !TelemetryConfiguration.IsOptOutEnvironmentVariableSet() )
        {
            this._logger.Trace?.Log( "Enabling telemetry." );

            this._configurationManager.Update<TelemetryConfiguration>(
                c => c with
                {
                    // Enable telemetry except if it has been disabled by the command line.
                    ExceptionReportingAction = c.ExceptionReportingAction == ReportingAction.Ask ? ReportingAction.Yes : c.ExceptionReportingAction,
                    PerformanceProblemReportingAction =
                    c.PerformanceProblemReportingAction == ReportingAction.Ask ? ReportingAction.Yes : c.PerformanceProblemReportingAction,
                    UsageReportingAction = c.UsageReportingAction == ReportingAction.Ask ? ReportingAction.Yes : c.UsageReportingAction
                } );
        }
    }

    private void OpenWelcomePage()
    {
        try
        {
            var applicationInfo = this._serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;

            var url = applicationInfo.IsPreviewLicenseEligible()
                ? "https://www.postsharp.net/links/metalama-welcome-preview"
                : "https://www.postsharp.net/links/metalama-welcome";

            this._logger.Trace?.Log( $"Opening '{url}'." );

            _ = this._processExecutor.Start( new ProcessStartInfo( url ) { UseShellExecute = true } );
        }
        catch ( Exception e )
        {
            this._logger.Error?.Log( $"Cannot start the welcome web page: {e.Message}" );

#if DEBUG
            throw;
#endif
        }
    }
}