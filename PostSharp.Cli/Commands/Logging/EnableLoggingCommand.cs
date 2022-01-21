// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Cli.Commands.Logging;

internal class EnableLoggingCommand : CommandBase
{
    public EnableLoggingCommand(
        ICommandServiceProvider commandServiceProvider,
        string name,
        string? description = null ) : base( commandServiceProvider, name, description ) { }
}