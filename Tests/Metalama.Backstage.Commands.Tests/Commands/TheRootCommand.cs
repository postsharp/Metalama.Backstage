using Metalama.DotNetTools;
using Metalama.DotNetTools.Commands;
using System.CommandLine;

namespace Metalama.Tools.Config.Tests.Commands;

internal class TheRootCommand : RootCommand
{
    public TheRootCommand(ICommandServiceProviderProvider serviceProviderProvider)
    {
        foreach ( var command in BackstageCommandFactory.CreateCommands( serviceProviderProvider ) )
        {
            this.Add( command );
        }
    }
}