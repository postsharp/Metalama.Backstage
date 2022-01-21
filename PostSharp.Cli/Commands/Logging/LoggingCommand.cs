// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Cli.Commands.Logging;

internal class LoggingCommand : CommandBase
{
    public LoggingCommand( ICommandServiceProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "logging",
        "Configure the logging of PostSharp itself." )
    {
        this.AddCommand( new ResetLoggingCommand( commandServiceProvider ) );
        this.AddCommand( new PrintLoggingCommand( commandServiceProvider ) );
        this.AddCommand( new EditLoggingCommand( commandServiceProvider ) );
    }
}