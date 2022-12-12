// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Commands.Commands.Telemetry;

internal class TelemetryCommand : CommandBase
{
    public TelemetryCommand( ICommandServiceProviderProvider commandServiceProvider )
        : base( commandServiceProvider, "telemetry", "Manages telemetry" )
    {
        this.Add( new EnableTelemetryCommand( commandServiceProvider, "enable", "Enables telemetry", true ) );
        this.Add( new EnableTelemetryCommand( commandServiceProvider, "disable", "Disables telemetry", false ) );
        this.Add( new ResetDeviceIdCommand( commandServiceProvider ) );
        this.Add( new UploadTelemetryCommand( commandServiceProvider ) );
    }
}