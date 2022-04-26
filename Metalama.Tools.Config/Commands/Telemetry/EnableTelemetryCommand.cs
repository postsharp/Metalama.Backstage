// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

    public EnableTelemetryCommand( ICommandServiceProvider commandServiceProvider, string name, string description, bool enable ) : base(
        commandServiceProvider,
        name,
        description )
    {
        this._enable = enable;
        this.Handler = CommandHandler.Create<IConsole>( this.Execute );
    }

    private void Execute( IConsole console )
    {
        var services = this.CommandServiceProvider.Initialize( console, false );
        var configurationManager = services.GetRequiredService<IConfigurationManager>();
        var reportAction = this._enable ? ReportingAction.Yes : ReportingAction.No;

        configurationManager.Update<TelemetryConfiguration>(
            c =>
            {
                c.ReportUsage = reportAction;
                c.ExceptionReportingAction = reportAction;
                c.PerformanceProblemReportingAction = reportAction;
            } );

        var state = this._enable ? "enabled" : "disabled";
        console.Out.WriteLine( $"Telemetry has been {state}." );
    }
}