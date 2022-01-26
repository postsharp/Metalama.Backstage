// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Telemetry;

[ConfigurationFile( "telemetry.json" )]
internal class TelemetryConfiguration : ConfigurationFile
{
    public ReportingAction ErrorReportingAction { get; set; } = ReportingAction.Yes;

    public ReportingAction NewExceptionReportingAction { get; set; } = ReportingAction.Yes;

    public ReportingAction NewPerformanceProblemReportingAction { get; set; } = ReportingAction.Yes;

    public Guid DeviceId { get; set; } = Guid.NewGuid();

    public DateTime? LastUploadTime { get; set; }

    public Dictionary<string, ReportingStatus> Issues { get; private set; } = new( StringComparer.OrdinalIgnoreCase );

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
        this.ErrorReportingAction = source.ErrorReportingAction;
        this.NewExceptionReportingAction = source.NewExceptionReportingAction;
        this.NewPerformanceProblemReportingAction = source.NewPerformanceProblemReportingAction;
        this.DeviceId = source.DeviceId;
        this.LastUploadTime = source.LastUploadTime;
        this.Issues = new Dictionary<string, ReportingStatus>( source.Issues );
    }
}

internal enum ReportingStatus
{
    None,
    Reported,
    Ignored
}