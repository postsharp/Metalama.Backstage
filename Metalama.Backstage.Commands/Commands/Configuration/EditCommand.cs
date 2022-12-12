// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Linq;

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
        this.CommandServices.Initialize( console, verbose );
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();
        configurationManager.CreateIfMissing<DiagnosticsConfiguration>();
        var configurationType = BackstageCommandFactory.ConfigurationCategories[name].GetType();
        // TODO: configurationManager.CreateIfMissing( configurationType );

        var filePath = configurationManager.GetFilePath( configurationType );
        console.Out.Write( $"Opening '{filePath}' in the default editor." );

        Process.Start( new ProcessStartInfo( filePath ) { UseShellExecute = true } );
    }
}