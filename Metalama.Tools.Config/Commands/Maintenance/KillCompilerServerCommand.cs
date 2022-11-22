// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage;
using Metalama.Backstage.Maintenance;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Metalama.DotNetTools.Commands.Maintenance;

internal class KillCompilerServerCommand : CommandBase
{
    public KillCompilerServerCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "kill",
        "Shuts down or kills locking compiler processes" )
    {
        this.AddOption( new Option( new[] { "--no-warn" }, "Do not write warnings for processes that may be locking Metalama files." ) );
        this.Handler = CommandHandler.Create<bool, bool, IConsole>( this.Execute );
    }

    private void Execute( bool noWarn, bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        var processManager = this.CommandServices.ServiceProvider.GetRequiredService<IProcessManager>();

        processManager.KillCompilerProcesses( !noWarn );
    }
}