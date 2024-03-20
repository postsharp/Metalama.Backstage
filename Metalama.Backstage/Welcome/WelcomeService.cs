// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.UserInterface;
using System;

namespace Metalama.Backstage.Welcome;

public class WelcomeService : IBackstageService
{
    private readonly ILogger _logger;
    private readonly IConfigurationManager _configurationManager;
    private readonly Guid? _newDeviceId;
    private readonly WebLinks _webLinks;

    public bool WelcomePageDisplayed
    {
        get => this._configurationManager.Get<WelcomeConfiguration>().WelcomePageDisplayed;
        set => this._configurationManager.Update<WelcomeConfiguration>( c => c with { WelcomePageDisplayed = value } );
    }

    public WelcomeService( IServiceProvider serviceProvider ) : this( serviceProvider, null ) { }

    // For test only.
    internal WelcomeService( IServiceProvider serviceProvider, Guid? newDeviceId )
    {
        this._newDeviceId = newDeviceId;
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "Welcome" );
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._webLinks = serviceProvider.GetRequiredBackstageService<WebLinks>();
    }

    private void ExecuteOnce(
        Action action,
        string actionName,
        Func<WelcomeConfiguration, bool> getIsFirst,
        Func<WelcomeConfiguration, WelcomeConfiguration> setIsNotFirst )
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

    public string? GetWelcomePageUrlAndRemember()
    {
        var url = this.WelcomePageDisplayed ? null : this._webLinks.AfterSetup;
        this.WelcomePageDisplayed = true;

        return url;
    }

    public void Initialize()
    {
        this.ExecuteOnce(
            this.ActivateTelemetry,
            nameof(this.ActivateTelemetry),
            c => c.IsFirstStart,
            c => c with { IsFirstStart = false } );
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
                    DeviceId = this._newDeviceId ?? Guid.NewGuid(),
                    ExceptionReportingAction = c.ExceptionReportingAction == ReportingAction.Ask ? ReportingAction.Yes : c.ExceptionReportingAction,
                    PerformanceProblemReportingAction =
                    c.PerformanceProblemReportingAction == ReportingAction.Ask ? ReportingAction.Yes : c.PerformanceProblemReportingAction,
                    UsageReportingAction = c.UsageReportingAction == ReportingAction.Ask ? ReportingAction.Yes : c.UsageReportingAction
                } );
        }
    }
}