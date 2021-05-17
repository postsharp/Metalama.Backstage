﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Cli.Console;

namespace PostSharp.Cli
{
    internal class CommandServiceProvider : ICommandServiceProvider
    {

        public IServiceProvider CreateServiceProvider( IConsole console, bool addTrace )
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<IDiagnosticsSink>( new ConsoleDiagnosticsSink( console ) )
                .AddSingleton<IDateTimeProvider>( new CurrentDateTimeProvider() )
                .AddSingleton<IFileSystem>( new FileSystem() );

            if ( addTrace )
            {
                serviceCollection
                    
                    // https://docs.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
                    .AddLogging( builder => builder.AddConsole() )
                    
                    // https://www.blinkingcaret.com/2018/02/14/net-core-console-logging/
                    .Configure<LoggerFilterOptions>( options => options.MinLevel = LogLevel.Trace );
            }

            return serviceCollection.BuildServiceProvider();
        }
    }
}
