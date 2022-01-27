// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Backstage.Telemetry;

[ConfigurationFile( "telemetry.json" )]
public class TelemetryConfiguration : ConfigurationFile
{
    public static bool IsOptOutEnvironmentVariableSet() => !string.IsNullOrEmpty( Environment.GetEnvironmentVariable( "METALAMA_TELEMETRY_OPT_OUT" ) );

    public ReportingAction ExceptionReportingAction { get; set; } = ReportingAction.Ask;

    public ReportingAction PerformanceProblemReportingAction { get; set; } = ReportingAction.Ask;

    public Guid DeviceId { get; set; } = Guid.NewGuid();

    public DateTime? LastUploadTime { get; set; }

    public Dictionary<string, ReportingStatus> Issues { get; private set; } = new( StringComparer.OrdinalIgnoreCase );

    public Dictionary<string, DateTime> Sessions { get; private set; } = new( StringComparer.OrdinalIgnoreCase );

    public ReportingAction ReportUsage { get; set; } = ReportingAction.Ask;

    public void CleanUp( DateTime threshold )
    {
        var sessionsToRemove = this.Sessions.Where( s => s.Value < threshold ).Select( s => s.Key );

        foreach ( var sessionToRemove in sessionsToRemove )
        {
            this.Sessions.Remove( sessionToRemove );
        }
    }

    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (TelemetryConfiguration) configurationFile;
        this.ExceptionReportingAction = source.ExceptionReportingAction;
        this.PerformanceProblemReportingAction = source.PerformanceProblemReportingAction;
        this.ReportUsage = source.ReportUsage;
        this.DeviceId = source.DeviceId;
        this.LastUploadTime = source.LastUploadTime;
        this.Issues = new Dictionary<string, ReportingStatus>( source.Issues );
        this.Sessions = new Dictionary<string, DateTime>( source.Sessions );
    }
}

public enum ReportingStatus
{
    None,
    Reported,
    Ignored
}