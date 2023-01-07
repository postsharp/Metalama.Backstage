// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.MicrosoftLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;

namespace Metalama.Backstage.Commands
{
    internal class CommandServiceProvider : ICommandServiceProviderProvider
    {
        public IServiceProvider GetServiceProvider( ConsoleWriter console, bool verbose )
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod

            var serviceCollection = new ServiceCollection();

            serviceCollection

                // https://docs.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
                .AddLogging(
                    builder =>
                    {
                        builder.Services.TryAddEnumerable( ServiceDescriptor.Singleton<ILoggerProvider>( new AnsiConsoleLoggingProvider( verbose, console ) ) );
                    } )

                // https://www.blinkingcaret.com/2018/02/14/net-core-console-logging/
                .Configure<LoggerFilterOptions>( options => options.MinLevel = verbose ? LogLevel.Trace : LogLevel.Information );

            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();

            var serviceProviderBuilder = new ServiceProviderBuilder(
                ( type, instance ) => serviceCollection.AddSingleton( type, instance ),
                () => serviceCollection.BuildServiceProvider() );

            var initializationOptions = new BackstageInitializationOptions( new MetalamaConfigApplicationInfo() )
            {
                AddLicensing = true,
                AddSupportServices = true,
                AddLoggerFactoryAction = builder =>
                {
                    if ( loggerFactory != null )
                    {
                        builder.AddMicrosoftLoggerFactory( loggerFactory );
                    }
                }
            };

            serviceProviderBuilder.AddBackstageServices( initializationOptions );

            return serviceCollection.BuildServiceProvider();
        }
    }
}