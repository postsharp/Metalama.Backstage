// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Logging;

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
        var configuration = services.GetRequiredService<IConfigurationManager>().Get<DiagnosticsConfiguration>();

        console.Out.WriteLine( $"The file '{configuration.FilePath}' contains the following configuration:" );
        console.Out.WriteLine();
        console.Out.WriteLine( configuration.ToJson() );
    }
}