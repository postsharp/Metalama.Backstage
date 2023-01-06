// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Metalama.Backstage.Commands.Commands.Telemetry;

internal class ResetDeviceIdCommand : CommandBase<CommonCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, CommonCommandSettings settings )
    {
        context.ServiceProvider.GetRequiredService<IConfigurationManager>()
            .Update<TelemetryConfiguration>( c => c with { DeviceId = Guid.NewGuid() } );

        context.Console.WriteSuccess( "The device id has been reset." );
    }
}