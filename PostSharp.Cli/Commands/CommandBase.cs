// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Cli.Commands
{
    internal class CommandBase : Command
    {
        protected IServiceProvider Services { get; }

        protected ITrace Trace { get; }

        public CommandBase( IServiceProvider services, ITrace trace, string name, string? description = null )
            : base( name, description )
        {
            this.Services = services;
            this.Trace = trace;
        }
    }
}
