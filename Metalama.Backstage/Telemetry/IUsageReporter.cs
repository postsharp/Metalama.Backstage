// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.Telemetry;

public interface IUsageReporter : IBackstageService
{
    bool ShouldReportSession( string projectName );

    IUsageSample CreateSample( string kind );
}