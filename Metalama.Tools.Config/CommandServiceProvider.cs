// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.MicrosoftLogging;
using Metalama.Backstage.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Metalama.DotNetTools
{
    internal class CommandServiceProvider : ICommandServiceProviderProvider
    {
        private IServiceProvider? _serviceProvider;

        public IServiceProvider ServiceProvider => this._serviceProvider ?? throw new InvalidOperationException( "Command services have not been initialized." );
        
        public void Initialize( IConsole console, bool addTrace )
        {
            if ( this.ServiceProvider != null )
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

            serviceProviderBuilder
                .AddSingleton<IConsole>( console )
                .AddMinimalBackstageServices( new MetalamaConfigApplicationInfo() );

            if ( loggerFactory != null )
            {
                serviceProviderBuilder.AddMicrosoftLoggerFactory( loggerFactory );
            }

            serviceProviderBuilder.AddTelemetryServices();

            var usageSample = serviceProviderBuilder.ServiceProvider.GetService<IUsageReporter>()?.CreateSample( "CompilerUsage" );

            if ( usageSample != null )
            {
                serviceProviderBuilder.AddSingleton<IUsageSample>( usageSample );
            }

            this._serviceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}