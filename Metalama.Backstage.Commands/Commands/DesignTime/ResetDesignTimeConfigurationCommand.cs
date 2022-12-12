// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Metalama.Backstage.Commands.Commands.DesignTime;

internal class ResetDesignTimeConfigurationCommand : CommandBase
{
    public ResetDesignTimeConfigurationCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "reset",
        "Resets the current design-time configuration to the default state" )
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

        if ( fileSystem.FileExists( filePath ) )
        {
            fileSystem.DeleteFile( filePath );
        }
    }
}