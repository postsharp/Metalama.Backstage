// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.Backstage.Commands.Commands.Configuration;

internal class PrintCommand : CommandBase
{
    public PrintCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "print",
        "Prints the specified configuration" )
    {
        this.AddArgument(
            new Argument<string>(
                "name",
                "Name of the configuration category to be printed" ) );

        this.Handler = CommandHandler.Create<string, bool, IConsole>( this.Execute );
    }

    private void Execute( string name, bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();

        if ( !BackstageCommandFactory.ConfigurationCategories.ContainsKey( name ) )
        {
            console.Out.WriteLine( $"Argument '{name}' is not recognized configuration name. Use 'metalama config list' command to see list of accepted names." );

            return;
        }

        var configurationType = BackstageCommandFactory.ConfigurationCategories[name].GetType();
        var configuration = configurationManager.Get( configurationType );
        var filePath = configurationManager.GetFilePath( configurationType );

        console.Out.WriteLine( $"The file '{filePath}' contains the following configuration:" );
        console.Out.WriteLine();
        console.Out.WriteLine( configuration.ToJson() );
    }
}