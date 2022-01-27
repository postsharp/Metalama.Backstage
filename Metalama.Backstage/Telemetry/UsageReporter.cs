// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Telemetry;

internal class UsageReporter : IUsageReporter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TelemetryConfiguration _configuration;
    private readonly UploadManager _uploadManager;
    private readonly IConfigurationManager _configurationManager;
    private readonly IDateTimeProvider _time;
    private readonly ILogger _logger;

    public UsageReporter( UploadManager uploadManager, IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();
        this._configuration = this._configurationManager.Get<TelemetryConfiguration>();
        this._uploadManager = uploadManager;
        this._time = serviceProvider.GetRequiredService<IDateTimeProvider>();
        this._logger = serviceProvider.GetLoggerFactory().Telemetry();
    }

    public bool ShouldReportSession( string projectName )
    {
        var now = this._time.Now;

        if ( TelemetryConfiguration.IsOptOutEnvironmentVariableSet() )
        {
            this._logger.Trace?.Log( $"Session of project '{projectName}' should not be reported because the opt-out environment variable is set." );

            return false;
        }

        if ( this._configuration.Sessions.TryGetValue( projectName, out var lastReported ) && lastReported.AddDays( 1 ) < now )
        {
            this._logger.Trace?.Log( $"Session of project '{projectName}' should not be reported because it has been reported on {lastReported}." );

            return false;
        }

        return this._configurationManager.Update<TelemetryConfiguration>(
            c =>
            {
                if ( c.Sessions.TryGetValue( projectName, out var raceReported ) && raceReported.AddDays( 1 ) < now )
                {
                    this._logger.Trace?.Log(
                        $"Session of project '{projectName}' should not be reported because it is being reported by a concurrent process." );

                    return false;
                }

                c.Sessions[projectName] = now;
                c.CleanUp( now.AddDays( -1 ) );

                this._logger.Trace?.Log( $"Session of project '{projectName}' should be reported." );

                return true;
            } );
    }

    public IUsageSample CreateSample( string kind ) => new UsageSample( this._serviceProvider, this._configuration, kind, this._uploadManager );
}