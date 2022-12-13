// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

namespace Metalama.Backstage.Commands.Commands.Configuration;

internal class EditCommand : CommandBase
{
    public EditCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "edit",
        "Edits the specified configuration with the default editor for JSON files" )
    {
        this.AddArgument(
            new Argument<string>(
                "name",
                "Name of the configuration category to be edited" ) );

        this.Handler = CommandHandler.Create<string, bool, IConsole>( this.Execute );
    }

    private void Execute( string name, bool verbose, IConsole console )
    {
        if ( !ConfigurationCommand.VerifyArgumentExistsInDictionary( name, console ) )
        {
            return;
        }

        this.CommandServices.Initialize( console, verbose );
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();
        var configurationType = BackstageCommandFactory.ConfigurationFilesByCategory[name];

        // TODO #32386: This needs to be generic and for all ConfigurationFiles.
        configurationManager.CreateIfMissing<DiagnosticsConfiguration>();

        var filePath = configurationManager.GetFilePath( configurationType.GetType() );
        console.Out.Write( $"Opening '{filePath}' in the default editor." );

        Process.Start( new ProcessStartInfo( filePath ) { UseShellExecute = true } );
    }
}