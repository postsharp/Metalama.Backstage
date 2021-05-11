// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using PostSharp.Backstage.Extensibility;
using PostSharp.Cli.Console;

namespace PostSharp.Cli
{
    internal class ServicesFactory : IServicesFactory
    {
        public (IServiceProvider Services, ITrace Trace) Create( IConsole console, bool verbose )
        {
            var services = new Services( console );
            ITrace trace = verbose ? new ConsoleTrace( console ) : new NullTrace();
            return (services, trace);
        }
    }
}
