// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

namespace Metalama.DotNetTools.Commands.Logging;

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
        var configurationManager = services.GetRequiredService<IConfigurationManager>();
        var configuration = configurationManager.Get<DiagnosticsConfiguration>();

        if ( configuration.LastModified == null )
        {
            // Create a default file if it does not exist.
            configurationManager.Update<DiagnosticsConfiguration>( _ => { } );
        }

        Process.Start( new ProcessStartInfo( configuration.FilePath ) { UseShellExecute = true } );
    }
}