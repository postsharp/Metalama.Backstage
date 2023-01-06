// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Telemetry;
using Microsoft.Extensions.DependencyInjection;

namespace Metalama.Backstage.Commands.Commands.Telemetry;

internal abstract class SetTelemetryCommand : CommandBase<CommonCommandSettings>
{
    private readonly bool _enable;

    public SetTelemetryCommand() : this( true ) { }

    public SetTelemetryCommand( bool enable )
    {
        this._enable = enable;
    }

    protected override void Execute( ExtendedCommandContext context, CommonCommandSettings settings )
    {
        var configurationManager = context.ServiceProvider.GetRequiredService<IConfigurationManager>();
        var reportAction = this._enable ? ReportingAction.Yes : ReportingAction.No;

        configurationManager.Update<TelemetryConfiguration>(
            c => c with { UsageReportingAction = reportAction, ExceptionReportingAction = reportAction, PerformanceProblemReportingAction = reportAction } );

        var state = this._enable ? "enabled" : "disabled";
        context.Console.WriteSuccess( $"Telemetry has been {state}." );
    }
}