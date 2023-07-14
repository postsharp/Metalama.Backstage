// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using Xunit.Abstractions;
using ILoggerFactory = Metalama.Backstage.Diagnostics.ILoggerFactory;

namespace Metalama.Backstage.Testing
{
    public abstract class TestsBase
    {
        private readonly IServiceCollection _serviceCollection;

        public ITestOutputHelper Logger { get; }

        public TestDateTimeProvider Time { get; } = new();

        public TestFileSystem FileSystem { get; }

        public TestEnvironmentVariableProvider EnvironmentVariableProvider { get; } = new();

        public TestLoggerFactory Log { get; }

        public NullTelemetryUploader TelemetryUploader { get; } = new();

        public TestUsageReporter UsageReporter { get; } = new();

        public IServiceProvider ServiceProvider { get; }

        public InMemoryConfigurationManager ConfigurationManager { get; }

        public TestProcessExecutor ProcessExecutor { get; } = new();

        protected IServiceCollection CreateServiceCollectionClone()
        {
            var services = new ServiceCollection();

            foreach ( var service in this._serviceCollection )
            {
                services.Add( service );
            }

            return services;
        }

        public TestsBase( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
        {
            this.Logger = logger;

            this.Log = new TestLoggerFactory( logger );

            // ReSharper disable RedundantTypeArgumentsOfMethod

            this._serviceCollection = new ServiceCollection()
                .AddSingleton<IDateTimeProvider>( this.Time )
                .AddSingleton<IProcessExecutor>( this.ProcessExecutor );

            this.FileSystem = new TestFileSystem( this._serviceCollection.BuildServiceProvider() );

            this._serviceCollection
                .AddSingleton<IEnvironmentVariableProvider>( this.EnvironmentVariableProvider )
                .AddSingleton<IRecoverableExceptionService>( new TestRecoverableExceptionService() )
                .AddSingleton<IFileSystem>( this.FileSystem );

            var serviceProviderBuilder =
                new ServiceProviderBuilder(
                    ( type, instance ) => this._serviceCollection.AddSingleton( type, instance ),
                    () => this._serviceCollection.BuildServiceProvider() );

            serviceProviderBuilder.AddService( typeof(ILoggerFactory), this.Log );
            serviceProviderBuilder.AddStandardDirectories();

            this._serviceCollection.AddSingleton<ITelemetryUploader>( this.TelemetryUploader );
            this._serviceCollection.AddSingleton<IUsageReporter>( this.UsageReporter );

            this.ConfigurationManager = new InMemoryConfigurationManager( this._serviceCollection.BuildServiceProvider() );

            this._serviceCollection.AddSingleton<IConfigurationManager>( this.ConfigurationManager );

            // ReSharper restore RedundantTypeArgumentsOfMethod

            serviceBuilder?.Invoke( serviceProviderBuilder );

            this.ServiceProvider = this._serviceCollection.BuildServiceProvider();
        }
    }
}