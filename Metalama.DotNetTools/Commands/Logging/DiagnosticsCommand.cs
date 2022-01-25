// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.DotNetTools.Commands.Logging;

internal class DiagnosticsCommand : CommandBase
{
    public DiagnosticsCommand( ICommandServiceProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "diag",
        "Configure the diagnostics settings of Metalama" )
    {
        this.AddCommand( new ResetLoggingCommand( commandServiceProvider ) );
        this.AddCommand( new PrintLoggingCommand( commandServiceProvider ) );
        this.AddCommand( new EditLoggingCommand( commandServiceProvider ) );
    }
}