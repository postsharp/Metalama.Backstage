// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

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

        var welcomeService = new WelcomeService( this.CommandServices.ServiceProvider );
        welcomeService.ExecuteFirstStartSetup();
        welcomeService.OpenWelcomePage();
    }
}