// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

namespace Metalama.DotNetTools.Commands.Logging;

internal class EditLoggingCommand : CommandBase
{
    public EditLoggingCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "edit",
        "Edits the logging configuration with the default editor for JSON files" )
    {
        this.Handler = CommandHandler.Create<IConsole>( this.Execute );
    }

    private void Execute( IConsole console )
    {
        this.CommandServices.Initialize( console, false );
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();
        var configuration = configurationManager.Get<DiagnosticsConfiguration>();

        if ( configuration.LastModified == null )
        {
            // Create a default file if it does not exist.
            configurationManager.Update<DiagnosticsConfiguration>( _ => { } );
        }

        console.Out.Write( $"Opening $'{configuration.FilePath}' in the default editor." );

        Process.Start( new ProcessStartInfo( configuration.FilePath ) { UseShellExecute = true } );
    }
}