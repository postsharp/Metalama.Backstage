// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage;
using Metalama.Backstage.Maintenance;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Metalama.DotNetTools.Commands.Maintenance;

internal class CleanUpCommand : CommandBase
{
    public CleanUpCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "cleanup",
        "Cleans up cache directory" )
    {
        this.AddOption( new Option( new[] { "--all" }, "Delete all directories and files ignoring clean-up policies" ) );
        this.AddOption( new Option( new[] { "--no-kill" }, "Disables automatic VBCSCompiler process killing before clean-up." ) );
        this.Handler = CommandHandler.Create<bool, bool, bool, IConsole>( this.Execute );
    }

    private void Execute( bool all, bool noKill, bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        if ( !noKill )
        {
            // Automatically kill VBCSCompiler processes before Cleanup unless --no-kill option is used.
            var processManager = this.CommandServices.ServiceProvider.GetRequiredService<IProcessManager>();
            processManager.KillCompilerProcesses();
        }

        var tempFileManager = new TempFileManager( this.CommandServices.ServiceProvider );

        tempFileManager.CleanTempDirectories( true, all );
    }
}