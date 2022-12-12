// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.Backstage.Commands.Commands.Configuration;

internal class ListCommand : CommandBase
{
    public ListCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "list",
        "Lists all possible <name> arguments for 'metalama config' commands." )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
    }

    private void Execute( bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        console.Out.WriteLine( "The 'metalama config' commands accept of the following <name> arguments:" );
        console.Out.WriteLine();

        foreach ( var category in BackstageCommandFactory.ConfigurationCategories.Keys )
        {
            console.Out.WriteLine( $"{category}" );
        }
    }
}