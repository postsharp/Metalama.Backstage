// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Telemetry;

namespace Metalama.Backstage.Testing.Services;

// TODO: Unit tests for usage reporting.
public class TestUsageReporter : IUsageReporter
{
    public bool IsUsageReportingEnabled => false;

    public bool ShouldReportSession( string projectName ) => false;

    public void StartSession( string kind ) { }

    public MetricCollection Metrics { get; } = new();

    public void StopSession() { }
}