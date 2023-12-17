// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Metalama.Backstage.Commands
{
    internal class CommandServiceProvider : ICommandServiceProviderProvider
    {
        private readonly IApplicationInfo _applicationInfo;

        public CommandServiceProvider( IApplicationInfo applicationInfo )
        {
            this._applicationInfo = applicationInfo;
        }

        public IServiceProvider GetServiceProvider( ConsoleWriter console, BaseCommandSettings settings )
        {
            // ReSharper disable RedundantTypeArgumentsOfMethod

            var serviceCollection = new ServiceCollection();

            var serviceProviderBuilder = new ServiceProviderBuilder(
                ( type, func ) => serviceCollection.Add( new ServiceDescriptor( type, func, ServiceLifetime.Singleton ) ) );

            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            serviceProviderBuilder.AddService( typeof(ILoggerFactory), new AnsiConsoleLoggerFactory( console, settings ) );

            var initializationOptions = new BackstageInitializationOptions( this._applicationInfo )
            {
                AddLicensing = true, AddSupportServices = true, CreateLoggingFactory = _ => loggerFactory
            };

            serviceProviderBuilder.AddBackstageServices( initializationOptions );

            return serviceCollection.BuildServiceProvider();
        }
    }
}