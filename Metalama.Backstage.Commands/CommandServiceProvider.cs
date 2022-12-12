﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.MicrosoftLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;

namespace Metalama.Backstage.Commands
{
    public class CommandServiceProvider : ICommandServiceProviderProvider
    {
        private IServiceProvider? _serviceProvider;

        public IServiceProvider ServiceProvider
            => this._serviceProvider ?? throw new InvalidOperationException( "Command services have not been initialized." );

        public void Initialize( IConsole console, bool verbose )
        {
            if ( this._serviceProvider != null )
            {
                throw new InvalidOperationException( "Service provider is initialized already." );
            }

            // ReSharper disable RedundantTypeArgumentsOfMethod

            var serviceCollection = new ServiceCollection();

            serviceCollection

                // https://docs.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
                .AddLogging( builder => builder.AddConsole() )

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

            this._serviceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}