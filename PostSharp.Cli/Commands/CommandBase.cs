// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Cli.Console;
using System;
using System.CommandLine;

namespace PostSharp.Cli.Commands
{
    internal class CommandBase : Command
    {
        public CommandBase( string name, string? description = null )
            : base( name, description )
        {
        }

        protected (IServiceProvider Services, ITrace Trace) CreateServices( IConsole console, bool verbose )
        {
            var services = new Services( console );
            ITrace trace = verbose ? new ConsoleTrace( console ) : new NullTrace();
            return (services, trace);
        }
    }
}
