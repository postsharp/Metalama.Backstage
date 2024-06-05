// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Telemetry;
using System;

namespace Metalama.Backstage.Testing;

public class NullUsageReporter : IUsageReporter
{
    public bool IsUsageReportingEnabled => false;

    public bool ShouldReportSession( string projectName ) => false;

    public IDisposable? StartSession( string kind ) => null;

    public MetricCollection? Metrics => null;
}