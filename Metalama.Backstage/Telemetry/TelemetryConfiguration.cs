﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Backstage.Telemetry;

[ConfigurationFile( "telemetry.json" )]
public record TelemetryConfiguration : ConfigurationFile
{
    public static bool IsOptOutEnvironmentVariableSet( IEnvironmentVariableProvider environmentVariables )
        => !string.IsNullOrEmpty( environmentVariables.GetEnvironmentVariable( "METALAMA_TELEMETRY_OPT_OUT" ) );

    public ReportingAction ExceptionReportingAction { get; init; } = ReportingAction.Ask;

    public ReportingAction PerformanceProblemReportingAction { get; init; } = ReportingAction.Ask;

    public Guid DeviceId { get; init; } = Guid.NewGuid();

    public DateTime? LastUploadTime { get; init; }

    public ImmutableDictionary<string, ReportingStatus> Issues { get; init; } =
        ImmutableDictionary<string, ReportingStatus>.Empty.WithComparers( StringComparer.OrdinalIgnoreCase );

    public ImmutableDictionary<string, DateTime> Sessions { get; init; } =
        ImmutableDictionary<string, DateTime>.Empty.WithComparers( StringComparer.OrdinalIgnoreCase );

    public ReportingAction UsageReportingAction { get; init; } = ReportingAction.Ask;

    public TelemetryConfiguration CleanUp( DateTime threshold )
    {
        return this with
        {
            Sessions = this.Sessions.Where( s => s.Value.Date >= threshold ).ToImmutableDictionary( k => k.Key, k => k.Value, this.Sessions.KeyComparer )
        };
    }
}