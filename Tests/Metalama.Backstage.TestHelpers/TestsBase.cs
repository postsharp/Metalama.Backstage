// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using MELT;
using Metalama.Backstage.Configuration;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.MicrosoftLogging;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace Metalama.Backstage.Testing
{
    public abstract class TestsBase
    {
        public ITestOutputHelper Logger { get; }

        public TestDateTimeProvider Time { get; } = new();

        public TestFileSystem FileSystem { get; } = new();

        public TestEnvironmentVariableProvider EnvironmentVariableProvider { get; } = new();

        public ITestLoggerSink Log { get; }

        public TestTelemetryUploader TelemetryUploader { get; } = new();

        public TestUsageReporter UsageReporter { get; } = new();

        public IServiceProvider ServiceProvider { get; }

        public InMemoryConfigurationManager ConfigurationManager { get; }

        public TestsBase( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
        {
            this.Logger = logger;

            var loggerFactory = TestLoggerFactory
                .Create()
                .AddXUnit( logger );

            this.Log = loggerFactory.GetTestLoggerSink();

            // ReSharper disable RedundantTypeArgumentsOfMethod

            var serviceCollection = new ServiceCollection()
                .AddSingleton<IDateTimeProvider>( this.Time )
                .AddSingleton<IFileSystem>( this.FileSystem )
                .AddSingleton<IEnvironmentVariableProvider>( this.EnvironmentVariableProvider );

            var serviceProviderBuilder =
                new ServiceProviderBuilder(
                    ( type, instance ) => serviceCollection.AddSingleton( type, instance ),
                    () => serviceCollection.BuildServiceProvider() );

            serviceProviderBuilder
                .AddMicrosoftLoggerFactory( loggerFactory )
                .AddStandardDirectories();

            serviceCollection.AddSingleton<ITelemetryUploader>( this.TelemetryUploader );
            serviceCollection.AddSingleton<IUsageReporter>( this.UsageReporter );

            this.ConfigurationManager = new InMemoryConfigurationManager( serviceCollection.BuildServiceProvider() );

            serviceCollection.AddSingleton<IConfigurationManager>( this.ConfigurationManager );

            // ReSharper restore RedundantTypeArgumentsOfMethod

            serviceBuilder?.Invoke( serviceProviderBuilder );

            this.ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}