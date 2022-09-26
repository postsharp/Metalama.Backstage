// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Telemetry;

internal class EnableTelemetryCommand : CommandBase
{
    private readonly bool _enable;

    public EnableTelemetryCommand( ICommandServiceProviderProvider commandServiceProvider, string name, string description, bool enable ) : base(
        commandServiceProvider,
        name,
        description )
    {
        this._enable = enable;
        this.Handler = CommandHandler.Create<IConsole>( this.Execute );
    }

    private void Execute( IConsole console )
    {
        this.CommandServices.Initialize( console, false );
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();
        var reportAction = this._enable ? ReportingAction.Yes : ReportingAction.No;

        configurationManager.Update<TelemetryConfiguration>(
            c => c with { ReportUsage = reportAction, ExceptionReportingAction = reportAction, PerformanceProblemReportingAction = reportAction } );

        var state = this._enable ? "enabled" : "disabled";
        console.Out.WriteLine( $"Telemetry has been {state}." );
    }
}