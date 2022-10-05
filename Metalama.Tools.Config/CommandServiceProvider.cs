// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.MicrosoftLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;

namespace Metalama.DotNetTools
{
    internal class CommandServiceProvider : ICommandServiceProviderProvider
    {
        private IServiceProvider? _serviceProvider;

        public IServiceProvider ServiceProvider
            => this._serviceProvider ?? throw new InvalidOperationException( "Command services have not been initialized." );

        public void Initialize( IConsole console, bool addTrace )
        {
            if ( this._serviceProvider != null )
            {
                throw new InvalidOperationException( "Service provider is initialized already." );
            }

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

            serviceProviderBuilder.AddBackstageServices(
                new MetalamaConfigApplicationInfo(),
                addLoggerFactoryAction: builder =>
                {
                    if ( loggerFactory != null )
                    {
                        builder.AddMicrosoftLoggerFactory( loggerFactory );
                    }
                } );

            this._serviceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}