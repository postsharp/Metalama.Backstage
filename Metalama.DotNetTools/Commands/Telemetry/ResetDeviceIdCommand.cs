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
    public ResetDeviceIdCommand( ICommandServiceProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "reset-device-id",
        "Resets the device ID" )
    {
        this.Handler = CommandHandler.Create<IConsole>( this.Execute );
    }

    private void Execute( IConsole console )
    {
        var services = this.CommandServiceProvider.CreateServiceProvider( console, false );
        services.GetRequiredService<IConfigurationManager>().Update<TelemetryConfiguration>( c => c.DeviceId = Guid.NewGuid() );
        console.Out.WriteLine( "The device id has been reset." );
    }
}