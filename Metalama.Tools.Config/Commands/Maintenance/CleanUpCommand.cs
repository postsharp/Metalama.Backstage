// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Maintenance;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Maintenance;

internal class CleanUpCommand : CommandBase
{
    public CleanUpCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "cleanup",
        "Cleans up cache directory" )
    {
        this.Handler = CommandHandler.Create<bool, bool, IConsole>( this.Execute );

        var allOption = new Option<bool>( "--all", "Delete all directories and files ignoring cleanup policies" );
        this.AddOption( allOption );
    }

    private void Execute( bool all, bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );
        var tempFileManager = new TempFileManager( this.CommandServices.ServiceProvider );

        console.Out.WriteLine( "Cleaning Metalama cache files." );

        if ( all )
        {
            console.Out.WriteLine( "Ignoring cleanup policies." );
            tempFileManager.CleanAllDirectoriesIgnoringCleanUpPolicies();
        }
        else
        {
            console.Out.WriteLine( "Respecting cleanup policies." );
            tempFileManager.CleanDirectoriesRespectingCleanupPolicies();
        }
    }
}