// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;

namespace Metalama.Backstage.Commands.Commands.DesignTime;

internal class EditDesignTimeConfigurationCommand : CommandBase
{
    public EditDesignTimeConfigurationCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "edit",
        "Edits the design-time configuration with the default editor for JSON files" )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
    }

    private void Execute( bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );
        var fileSystem = this.CommandServices.ServiceProvider.GetRequiredService<IFileSystem>();
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();

        // The file name has to be kept consistent with Metalama.Framework.Engine.Configuration.DesignTimeConfiguration configuration class.
        var filePath = configurationManager.GetFilePath( "designTime.json" );

        if ( !fileSystem.FileExists( filePath ) )
        {
            console.Out.Write(
                $"The design-time configuration file '{filePath}' doesn't exist. Open Metalama project in s supported IDE to let the file be created." );

            return;
        }

        console.Out.Write( $"Opening '{filePath}' in the default editor." );

        Process.Start( new ProcessStartInfo( filePath ) { UseShellExecute = true } );
    }
}