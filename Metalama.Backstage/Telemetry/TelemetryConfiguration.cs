// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using System;
using System.Collections.Generic;

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

    public ReportingAction ReportUsage { get; set; } = ReportingAction.Ask;

    public bool MustReportIssue( string hash )
    {
        if ( this.Issues.TryGetValue( hash, out var currentStatus ) )
        {
            if ( currentStatus is ReportingStatus.Ignored or ReportingStatus.Reported )
            {
                return false;
            }
        }

        this.Issues[hash] = currentStatus;

        return true;
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
    }
}

public enum ReportingStatus
{
    None,
    Reported,
    Ignored
}