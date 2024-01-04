// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Telemetry;

internal class TelemetryConfigurationService : ITelemetryConfigurationService
{
    private readonly IServiceProvider _serviceProvider;

    public TelemetryConfigurationService( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }

    public void SetStatus( bool enabled )
    {
        var configurationManager = this._serviceProvider.GetRequiredBackstageService<IConfigurationManager>();
        var reportAction = enabled ? ReportingAction.Yes : ReportingAction.No;

        configurationManager.Update<TelemetryConfiguration>(
            c => c with { UsageReportingAction = reportAction, ExceptionReportingAction = reportAction, PerformanceProblemReportingAction = reportAction } );
    }
}