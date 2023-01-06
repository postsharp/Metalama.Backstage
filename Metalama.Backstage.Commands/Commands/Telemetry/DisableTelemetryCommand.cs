// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Commands.Commands.Telemetry;

internal class DisableTelemetryCommand : SetTelemetryCommand
{
    public DisableTelemetryCommand() : base( false ) { }
}