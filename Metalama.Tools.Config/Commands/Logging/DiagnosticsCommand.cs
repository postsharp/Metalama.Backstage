// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
        this.AddCommand( new SetLoggingHoursCommand( commandServiceProvider ) );
    }
}