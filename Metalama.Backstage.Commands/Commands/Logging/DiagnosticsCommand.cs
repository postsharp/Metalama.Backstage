// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Commands.Commands.Logging;

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