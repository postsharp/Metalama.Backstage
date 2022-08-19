// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Maintenance;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Maintenance;

internal class CleanUpCommand : CommandBase
{
    public CleanUpCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "cleanup",
        "Deletes folders" )
    {
        this.Handler = CommandHandler.Create<bool, bool, IConsole>( this.Execute );

        // TODO: Better description.
        var allOption = new Option<bool>( "--all", "Delete everything" );
        this.AddOption( allOption );
    }

    private void Execute( bool all, bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();
        var tempFileManager = (TempFileManager) this.CommandServices.ServiceProvider.GetRequiredService<ITempFileManager>();

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

        configurationManager.Update<CleanUpConfiguration>( c => c.Reset() );
    }
}