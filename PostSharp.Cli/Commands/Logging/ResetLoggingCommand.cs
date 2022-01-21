// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace PostSharp.Cli.Commands.Logging;

internal class ResetLoggingCommand : CommandBase
{
    public ResetLoggingCommand( ICommandServiceProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "reset",
        "Disables logging for all applications and all categories." )
    {
        this.Handler = CommandHandler.Create<IConsole>( this.Execute );
    }

    private void Execute( IConsole console )
    {
        var services = this.CommandServiceProvider.CreateServiceProvider( console, false );
        var configuration = LoggingConfiguration.Load( services );
        configuration.Reset();
        configuration.Save( services );
    }
}