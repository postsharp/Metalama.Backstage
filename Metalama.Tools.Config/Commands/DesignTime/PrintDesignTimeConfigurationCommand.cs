// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.DesignTime;

internal class PrintDesignTimeConfigurationCommand : CommandBase
{
    public PrintDesignTimeConfigurationCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "print",
        "Prints the current design-time configuration" )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
    }

    private void Execute( bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, false );
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

        console.Out.WriteLine( $"The file '{filePath}' contains the following configuration:" );
        console.Out.WriteLine();
        console.Out.WriteLine( fileSystem.ReadAllText( filePath ) );
    }
}