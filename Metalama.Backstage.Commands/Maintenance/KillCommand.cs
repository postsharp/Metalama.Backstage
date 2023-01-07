// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Maintenance;
using Microsoft.Extensions.DependencyInjection;

namespace Metalama.Backstage.Commands.Maintenance;

internal class KillCommand : BaseCommand<KillCommandSettings>
{
    protected override void Execute( ExtendedCommandContext context, KillCommandSettings settings )
    {
        context.Console.WriteHeading( "Killing Metalama processes" );

        var processManager = context.ServiceProvider.GetRequiredService<IProcessManager>();

        processManager.KillCompilerProcesses( !settings.NoWarn );

        context.Console.WriteSuccess( "Metalama processes have been killed." );
    }
}