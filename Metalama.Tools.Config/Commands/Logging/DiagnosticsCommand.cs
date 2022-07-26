// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.DotNetTools.Commands.Logging;

internal class DiagnosticsCommand : CommandBase
{
    public DiagnosticsCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "diag",
        "Manages logging and debugging options" )
    {
        this.AddCommand( new ResetLoggingCommand( commandServiceProvider ) );
        this.AddCommand( new PrintLoggingCommand( commandServiceProvider ) );
        this.AddCommand( new EditLoggingCommand( commandServiceProvider ) );
    }
}