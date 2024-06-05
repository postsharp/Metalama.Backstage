// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Utilities;
using System;
using System.Threading;

namespace Metalama.Backstage.Telemetry;

internal class UsageReporter : IUsageReporter
{
    private static readonly AsyncLocal<UsageTelemetryReport?> _currentSample = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly IConfigurationManager _configurationManager;
    private readonly IDateTimeProvider _time;
    private readonly ILogger _logger;
    private readonly TelemetryReportUploader _telemetryReportUploader;

    public bool IsUsageReportingEnabled { get; }

    public UsageReporter( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._configurationManager = serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        this._time = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();
        this._logger = serviceProvider.GetLoggerFactory().Telemetry();
        this._telemetryReportUploader = serviceProvider.GetRequiredBackstageService<TelemetryReportUploader>();

        var applicationInfo = serviceProvider.GetRequiredBackstageService<IApplicationInfoProvider>().CurrentApplication;

        if ( !applicationInfo.IsTelemetryEnabled )
        {
            this._logger.Trace?.Log(
                $"Usage should not be reported because telemetry is disabled for '{applicationInfo.Name} {applicationInfo.PackageVersion}'." );
        }
        else if ( TelemetryConfiguration.IsOptOutEnvironmentVariableSet( serviceProvider.GetRequiredBackstageService<IEnvironmentVariableProvider>() ) )
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

        var configuration = this._configurationManager.Get<TelemetryConfiguration>();

        if ( configuration.Sessions.TryGetValue( projectName, out var lastReported ) && lastReported.AddDays( 1 ) > now )
        {
            this._logger.Trace?.Log( $"Session of project '{projectName}' should not be reported because it has been reported on {lastReported}." );

            return false;
        }

        return this._configurationManager.UpdateIf<TelemetryConfiguration>(
            c =>
            {
                if ( c.Sessions.TryGetValue( projectName, out var raceReported ) && raceReported.AddDays( 1 ) > now )
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

                c = c.CleanUp( now.AddDays( -1 ) );
                c = c with { Sessions = c.Sessions.SetItem( projectName, now ) };

                return c;
            } );
    }

    public IDisposable? StartSession( string kind )
    {
        if ( !this.IsUsageReportingEnabled )
        {
            return null;
        }

        var previousSample = _currentSample.Value;
        var currentSample = new UsageTelemetryReport( this._serviceProvider, kind );
        _currentSample.Value = currentSample;

        if ( this._logger.Trace != null )
        {
            this._logger.Trace.Log( $"Usage session started." );
            this.TraceSample( currentSample );
        }

        return new DisposableAction( () => this.StopSession( previousSample, currentSample ) );
    }

    public MetricCollection? Metrics => _currentSample.Value?.Metrics;

    private void StopSession( UsageTelemetryReport? previousSample, UsageTelemetryReport currentSample )
    {
        if ( this._logger.Trace != null )
        {
            this._logger.Trace.Log( $"Usage session ended." );
            this.TraceSample( currentSample );
        }

        this._telemetryReportUploader.Upload( currentSample );
        _currentSample.Value = previousSample;
    }

    private void TraceSample( UsageTelemetryReport sample )
    {
        if ( this._logger.Trace == null )
        {
            return;
        }

        foreach ( var metric in sample.Metrics )
        {
            this._logger.Trace.Log( $"  {metric.Name}: {metric}" );
        }
    }
}