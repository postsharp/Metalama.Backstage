// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;

namespace Metalama.DotNetTools.Commands
{
    internal class CommandBase : Command
    {
        public ICommandServiceProviderProvider CommandServices { get; }

        public CommandBase( ICommandServiceProviderProvider commandServiceProvider, string name, string? description = null )
            : base( name, description )
        {
            this.CommandServices = commandServiceProvider;
        }
    }
}