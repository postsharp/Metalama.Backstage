// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Commands.Telemetry;

internal class EnableTelemetryCommand : SetTelemetryCommand
{
    public EnableTelemetryCommand() : base( true ) { }
}