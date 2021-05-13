// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using PostSharp.Backstage.Extensibility;
using PostSharp.Cli.Console;

namespace PostSharp.Cli
{
    internal class CommandServiceProvider : ICommandServiceProvider
    {

        public IServiceProvider CreateServiceProvider( IConsole console, bool addTrace )
        {
            ServiceProvider services = new();
            services.SetService<IDiagnosticsSink>( new ConsoleDiagnosticsSink( console ) );
            services.SetService<IDateTimeProvider>( new CurrentDateTimeProvider() );
            services.SetService<IFileSystem>( new FileSystem() );

            if ( addTrace )
            {
                services.SetService<ITrace>( new ConsoleTrace( console ) );
            }

            return services;
        }
    }
}
