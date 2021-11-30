// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.MicrosoftLogging;
using PostSharp.Cli.Console;
using System;
using System.CommandLine;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace PostSharp.Cli
{
    internal class CommandServiceProvider : ICommandServiceProvider
    {
        public IServiceProvider CreateServiceProvider( IConsole console, bool addTrace )
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod

            ServiceProvider? systemServiceProvider = null;
            Microsoft.Extensions.Logging.ILoggerFactory? loggerFactory = null;

            if ( addTrace )
            {
                var systemServiceCollection = new ServiceCollection();

                systemServiceCollection

                    // https://docs.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
                    .AddLogging( builder => builder.AddConsole() )

                    // https://www.blinkingcaret.com/2018/02/14/net-core-console-logging/
                    .Configure<LoggerFilterOptions>( options => options.MinLevel = LogLevel.Trace );

                systemServiceProvider = systemServiceCollection.BuildServiceProvider();
                loggerFactory = systemServiceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            }

            var serviceCollection = new BackstageServiceCollection( systemServiceProvider );

            serviceCollection
                .AddSingleton<IConsole>( console )
                .AddSingleton<IDiagnosticsSink>( services => new ConsoleDiagnosticsSink( services.ToServiceProvider() ) )
                .AddCurrentDateTimeProvider()
                .AddFileSystem()
                .AddStandardDirectories()
                .AddStandardLicenseFilesLocations();

            if ( loggerFactory != null )
            {
                serviceCollection.AddMicrosoftLoggerFactory( loggerFactory );
            }

            // ReSharper restore RedundantTypeArgumentsOfMethod

            return serviceCollection.ToServiceProvider();
        }
    }
}