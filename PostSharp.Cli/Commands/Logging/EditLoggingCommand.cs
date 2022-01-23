// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Diagnostics;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;

namespace PostSharp.Cli.Commands.Logging;

internal class EditLoggingCommand : CommandBase
{
    public EditLoggingCommand( ICommandServiceProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "edit",
        "Edits the logging configuration with the default editor for JSON files" )
    {
        this.Handler = CommandHandler.Create<IConsole>( this.Execute );
    }

    private void Execute( IConsole console )
    {
        var services = this.CommandServiceProvider.CreateServiceProvider( console, false );
        var configuration = DiagnosticsConfiguration.Load( services );

        if ( !File.Exists( configuration.FilePath ) )
        {
            // Create a default file if it does not exist.
            configuration.Save( services );
        }

        Process.Start( new ProcessStartInfo( configuration.FilePath ) { UseShellExecute = true } );
    }
}