// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using PostSharp.Backstage.Extensibility;
using PostSharp.Cli.Console;

namespace PostSharp.Cli.Commands
{
    internal class CommandBase : Command
    {
        protected IServiceProvider Services { get; }

        public CommandBase( IServiceProvider services, string name, string? description = null )
            : base( name, description )
        {
            this.Services = services;
        }

        // TODO add verbosity parameter

        protected static ITrace CreateTrace( InvocationContext context ) => new ConsoleTrace( context.Console );
    }
}
