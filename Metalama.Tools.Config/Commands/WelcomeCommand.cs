// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Welcome;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Metalama.DotNetTools.Commands;

internal class WelcomeCommand : CommandBase
{
    public WelcomeCommand( ICommandServiceProviderProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "welcome",
        "Executes the first-day initialization" )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        this.IsHidden = true;
    }

    private void Execute( bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );
        WelcomeService.Execute( this.CommandServices.ServiceProvider );
    }
}