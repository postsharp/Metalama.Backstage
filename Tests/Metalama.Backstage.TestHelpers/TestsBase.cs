// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using MELT;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.MicrosoftLogging;
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

        public ITestLoggerSink Log { get; }

        public IServiceProvider ServiceProvider { get; }

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
                .AddSingleton<IFileSystem>( this.FileSystem );

            var serviceProviderBuilder =
                new ServiceProviderBuilder(
                    ( type, instance ) => serviceCollection.AddSingleton( type, instance ),
                    () => serviceCollection.BuildServiceProvider() );

            serviceProviderBuilder
                .AddMicrosoftLoggerFactory( loggerFactory )
                .AddStandardDirectories();

            // ReSharper restore RedundantTypeArgumentsOfMethod

            serviceBuilder?.Invoke( serviceProviderBuilder );

            this.ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}