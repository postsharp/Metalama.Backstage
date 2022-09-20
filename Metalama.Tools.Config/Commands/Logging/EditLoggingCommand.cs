﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        configurationManager.Update<DiagnosticsConfiguration>( c => c.DisableLoggingForOutdatedSettings() );
        var configuration = configurationManager.Get<DiagnosticsConfiguration>();

        if ( configuration.LastModified == null )
        {
            // Create a default file if it does not exist.
            configurationManager.Update<DiagnosticsConfiguration>( c => c );
        }

        var fileName = configurationManager.GetFileName( typeof(DiagnosticsConfiguration) );
        console.Out.Write( $"Opening $'{fileName}' in the default editor." );

        Process.Start( new ProcessStartInfo( fileName ) { UseShellExecute = true } );
    }
}