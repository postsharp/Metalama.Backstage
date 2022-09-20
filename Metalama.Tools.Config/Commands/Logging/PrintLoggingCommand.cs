// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Logging;

internal class PrintLoggingCommand : CommandBase
{
    public PrintLoggingCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "print",
        "Prints the current logging configuration" )
    {
        this.Handler = CommandHandler.Create<IConsole>( this.Execute );
    }

    private void Execute( IConsole console )
    {
        this.CommandServices.Initialize( console, false );
        var configurationManager = this.CommandServices.ServiceProvider.GetRequiredService<IConfigurationManager>();
        var configuration = configurationManager.Get<DiagnosticsConfiguration>();
        var fileName = configurationManager.GetFileName( typeof(DiagnosticsConfiguration) );

        console.Out.WriteLine( $"The file '{fileName}' contains the following configuration:" );
        console.Out.WriteLine();
        console.Out.WriteLine( configuration.ToJson() );
    }
}