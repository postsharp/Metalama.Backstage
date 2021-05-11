// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;

namespace PostSharp.Cli.Commands
{
    internal class CommandBase : Command
    {
        public IServicesFactory ServicesFactory { get; }

        public CommandBase( IServicesFactory servicesFactory, string name, string? description = null )
            : base( name, description )
        {
            this.ServicesFactory = servicesFactory;
        }
    }
}
