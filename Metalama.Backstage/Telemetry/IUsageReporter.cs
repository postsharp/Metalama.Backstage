// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.Telemetry;

public interface IUsageReporter : IBackstageService
{
    bool IsUsageReportingEnabled { get; }

    bool ShouldReportSession( string projectName );

    void StartSession( string kind );

    MetricCollection Metrics { get; }

    void StopSession();
}