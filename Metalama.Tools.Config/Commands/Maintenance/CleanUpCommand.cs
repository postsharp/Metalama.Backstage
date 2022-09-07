// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Maintenance;
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

        this.Handler = CommandHandler.Create<bool, bool, IConsole>( this.Execute );
    }

    private void Execute( bool all, bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        var tempFileManager = new TempFileManager( this.CommandServices.ServiceProvider );

        tempFileManager.CleanTempDirectories( true, all );
    }
}