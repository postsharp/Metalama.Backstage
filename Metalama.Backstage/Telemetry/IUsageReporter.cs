﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Backstage.Telemetry;

public interface IUsageReporter
{
    bool ShouldReportSession( string projectName );

    IUsageSample CreateSample( string kind );
}