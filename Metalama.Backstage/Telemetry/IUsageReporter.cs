// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

namespace Metalama.Backstage.Telemetry;

public interface IUsageReporter
{
    bool ShouldReportSession( string projectName );

    IUsageSample CreateSample( string kind );
}