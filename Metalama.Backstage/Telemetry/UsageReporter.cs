// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Telemetry;

internal class UsageReporter : IUsageReporter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TelemetryConfiguration _configuration;
    private readonly IConfigurationManager _configurationManager;
    private readonly IDateTimeProvider _time;
    private readonly ILogger _logger;
    private UsageSample? _currentSample;

    public bool IsUsageReportingEnabled { get; }

    public UsageReporter( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._configuration = this._configurationManager.Get<TelemetryConfiguration>();
        this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._logger = serviceProvider.GetLoggerFactory().Telemetry();

        var applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;

        if ( !applicationInfo.IsTelemetryEnabled )
        {
            this._logger.Trace?.Log( $"Usage should not be reported because telemetry is disabled for {applicationInfo.Name} {applicationInfo.Version}." );
        }
        else if ( TelemetryConfiguration.IsOptOutEnvironmentVariableSet() )
        {
            this._logger.Trace?.Log( $"Usage should not be reported because the opt-out environment variable is set." );
        }
        else
        {
            this.IsUsageReportingEnabled = true;
        }
    }

    public bool ShouldReportSession( string projectName )
    {
        var now = this._time.Now;

        if ( !this.IsUsageReportingEnabled )
        {
            return false;
        }

        if ( this._configuration.Sessions.TryGetValue( projectName, out var lastReported ) && lastReported.AddDays( 1 ) < now )
        {
            this._logger.Trace?.Log( $"Session of project '{projectName}' should not be reported because it has been reported on {lastReported}." );

            return false;
        }

        return this._configurationManager.UpdateIf<TelemetryConfiguration>(
            c =>
            {
                if ( c.Sessions.TryGetValue( projectName, out var raceReported ) && raceReported.AddDays( 1 ) < now )
                {
                    this._logger.Trace?.Log(
                        $"Session of project '{projectName}' should not be reported because it is being reported by a concurrent process." );

                    return false;
                }

                return true;
            },
            c =>
            {
                this._logger.Trace?.Log( $"Session of project '{projectName}' should be reported." );

                return c.CleanUp( now.AddDays( -1 ) ) with { Sessions = c.Sessions.SetItem( projectName, now ) };
            } );
    }

    public bool StartSession( string kind )
    {
        if ( this._currentSample != null )
        {
            throw new InvalidOperationException();
        }

        if ( !this.IsUsageReportingEnabled )
        {
            return false;
        }

        this._currentSample = new UsageSample( this._serviceProvider, kind );

        return true;
    }

    public MetricCollection? Metrics => this._currentSample?.Metrics;

    public void StopSession()
    {
        if ( this._currentSample != null )
        {
            this._currentSample.Flush();
            this._currentSample = null;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}