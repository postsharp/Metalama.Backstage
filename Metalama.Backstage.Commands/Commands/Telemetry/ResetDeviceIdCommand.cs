// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.Backstage.Commands.Commands.Telemetry;

internal class ResetDeviceIdCommand : CommandBase
{
    public ResetDeviceIdCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "reset-device-id",
        "Resets the device ID" )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
    }

    private void Execute( bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>()
            .Update<TelemetryConfiguration>( c => c with { DeviceId = Guid.NewGuid() } );

        console.Out.WriteLine( "The device id has been reset." );
    }
}