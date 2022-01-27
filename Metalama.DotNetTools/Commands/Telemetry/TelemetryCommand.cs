// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.DotNetTools.Commands.Telemetry;

internal class TelemetryCommand : CommandBase
{
    public TelemetryCommand( ICommandServiceProvider commandServiceProvider )
        : base( commandServiceProvider, "telemetry", "Manages telemetry" )
    {
        this.Add( new EnableTelemetryCommand( commandServiceProvider, "enable", "Enables telemetry", true ) );
        this.Add( new EnableTelemetryCommand( commandServiceProvider, "disable", "Disables telemetry", false ) );
        this.Add( new ResetDeviceIdCommand( commandServiceProvider ) );
    }
}