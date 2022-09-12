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
    private readonly IApplicationInfo _applicationInfo;

    public UsageReporter( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._configuration = this._configurationManager.Get<TelemetryConfiguration>();
        this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._logger = serviceProvider.GetLoggerFactory().Telemetry();
        this._applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;
    }

    public bool IsUsageReportingEnabled()
    {
        if ( !this._applicationInfo.IsTelemetryEnabled )
        {
            this._logger.Trace?.Log(
                $"Usage should not be reported because telemetry is disabled for {this._applicationInfo.Name} {this._applicationInfo.Version}." );

            return false;
        }

        if ( TelemetryConfiguration.IsOptOutEnvironmentVariableSet() )
        {
            this._logger.Trace?.Log( $"Usage should not be reported because the opt-out environment variable is set." );

            return false;
        }

        return true;
    }

    public bool ShouldReportSession( string projectName )
    {
        var now = this._time.Now;

        if ( !this.IsUsageReportingEnabled() )
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
                c.Sessions = c.Sessions.SetItem( projectName, now );
                c.CleanUp( now.AddDays( -1 ) );

                this._logger.Trace?.Log( $"Session of project '{projectName}' should be reported." );
            } );
    }

    public IUsageSample CreateSample( string kind ) => new UsageSample( this._serviceProvider, kind );
}