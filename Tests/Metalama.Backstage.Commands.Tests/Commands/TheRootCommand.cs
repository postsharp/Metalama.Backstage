// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Metalama.Backstage.Commands.Commands;
using System.CommandLine;

namespace Metalama.Tools.Config.Tests.Commands;

internal class TheRootCommand : RootCommand
{
    public TheRootCommand( ICommandServiceProviderProvider serviceProviderProvider )
    {
        foreach ( var command in BackstageCommandFactory.CreateCommands( serviceProviderProvider ) )
        {
            this.Add( command );
        }
    }
}