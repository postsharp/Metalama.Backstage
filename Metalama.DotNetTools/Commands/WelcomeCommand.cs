using Metalama.Backstage.Welcome;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Metalama.DotNetTools.Commands;

internal class WelcomeCommand : CommandBase
{
    public WelcomeCommand( ICommandServiceProvider commandServiceProvider ) : base(
        commandServiceProvider,
        "welcome",
        "Executes the first-day initialization" )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        this.IsHidden = true;
    }

    private void Execute( bool verbose, IConsole console )
    {
        var services = this.CommandServiceProvider.CreateServiceProvider( console, verbose );
        WelcomeService.Execute( services );
    }
}