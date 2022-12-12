// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.CommandLine;

namespace Metalama.Backstage.Commands.Commands
{
    internal abstract class CommandBase : Command
    {
        public ICommandServiceProviderProvider CommandServices { get; }

        public CommandBase( ICommandServiceProviderProvider commandServiceProvider, string name, string? description = null )
            : base( name, description )
        {
            this.CommandServices = commandServiceProvider;
        }
    }
}