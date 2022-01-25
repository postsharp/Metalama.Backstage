// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.MicrosoftLogging;
using System;
using System.CommandLine;

namespace PostSharp.Cli
{
    internal class CommandServiceProvider : ICommandServiceProvider
    {
        public IServiceProvider CreateServiceProvider( IConsole console, bool addTrace )
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod

            var serviceCollection = new ServiceCollection();

            ILoggerFactory? loggerFactory = null;

            if ( addTrace )
            {
                serviceCollection

                    // https://docs.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
                    .AddLogging( builder => builder.AddConsole() )

                    // https://www.blinkingcaret.com/2018/02/14/net-core-console-logging/
                    .Configure<LoggerFilterOptions>( options => options.MinLevel = LogLevel.Trace );

                loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            }

            var serviceProviderBuilder = new ServiceProviderBuilder(
                ( type, instance ) => serviceCollection.AddSingleton( type, instance ),
                () => serviceCollection.BuildServiceProvider() );

            serviceProviderBuilder
                .AddSingleton<IConsole>( console )
                .AddMinimalBackstageServices();

            if ( loggerFactory != null )
            {
                serviceProviderBuilder.AddMicrosoftLoggerFactory( loggerFactory );
            }

            // ReSharper restore RedundantTypeArgumentsOfMethod

            return serviceCollection.BuildServiceProvider();
        }
    }
}