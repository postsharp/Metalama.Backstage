// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.IO;

namespace PostSharp.Cli.Commands.Logging;

internal class PrintLoggingCommand : CommandBase
{
    public PrintLoggingCommand( ICommandServiceProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "print",
        "Prints the current logging configuration" )
    {
        this.Handler = CommandHandler.Create<IConsole>( this.Execute );
    }

    private void Execute( IConsole console )
    {
        var services = this.CommandServiceProvider.CreateServiceProvider( console, false );
        var configuration = LoggingConfiguration.Load( services );

        if ( !File.Exists( configuration.FilePath ) )
        {
            configuration.Save( services );
        }

        console.Out.WriteLine( $"The file '{configuration.FilePath}' contains the following configuration:" );
        console.Out.WriteLine();
        console.Out.WriteLine( File.ReadAllText( configuration.FilePath ) );
    }
}