// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using SearchOption = System.IO.SearchOption;

namespace Metalama.DotNetTools.Commands.Maintenance;

internal class CleanUpCommand : CommandBase
{
    public CleanUpCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "cleanup",
        // TODO: Better description.
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

        if ( all )
        {
            console.Out.WriteLine( "Cleaning Metalama temporary files. Ignoring cleanup policies." );
            tempFileManager.CleanAllDirectoriesIgnoringCleanUpPolicies();
        }

        console.Out.WriteLine( "Cleaning Metalama temporary files following cleanup policies." );
        tempFileManager.CleanDirectories();

        configurationManager.Update<CleanUpConfiguration>( c => c.Reset() );
    }
}