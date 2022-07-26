// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Telemetry;

internal class ResetDeviceIdCommand : CommandBase
{
    public ResetDeviceIdCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "reset-device-id",
        "Resets the device ID" )
    {
        this.Handler = CommandHandler.Create<IConsole>( this.Execute );
    }

    private void Execute( IConsole console )
    {
        this.CommandServices.Initialize( console, false );
        this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>().Update<TelemetryConfiguration>( c => c.DeviceId = Guid.NewGuid() );
        console.Out.WriteLine( "The device id has been reset." );
    }
}