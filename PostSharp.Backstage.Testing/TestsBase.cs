using System;
using MELT;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.MicrosoftLogging;
using PostSharp.Backstage.Testing.Services;
using Xunit.Abstractions;
using ILoggerFactory = PostSharp.Backstage.Extensibility.ILoggerFactory;

namespace PostSharp.Backstage.Testing
{
    public abstract class TestsBase
    {
        private ITestOutputHelper _testOutputHelper;

        public TestDateTimeProvider Time { get; } = new();

        public TestFileSystem FileSystem { get; } = new();

        public ITestLoggerSink Log { get; }

        public IServiceProvider Services { get; }

        public TestsBase( ITestOutputHelper logger, Action<BackstageServiceCollection>? serviceBuilder = null )
        {
            var loggerFactory = TestLoggerFactory
                .Create()
                .AddXUnit( logger );

            Log = loggerFactory.GetTestLoggerSink();

            // ReSharper disable RedundantTypeArgumentsOfMethod

            var serviceCollection = new BackstageServiceCollection();

            serviceCollection
                .AddSingleton<ILoggerFactory>( new LoggerFactoryAdapter( loggerFactory ) )
                .AddSingleton<IDateTimeProvider>( Time )
                .AddSingleton<IFileSystem>( FileSystem );

            serviceCollection
                .AddStandardDirectories();

            // ReSharper restore RedundantTypeArgumentsOfMethod

            serviceBuilder?.Invoke( serviceCollection );

            Services = serviceCollection.ToServiceProvider();
        }
    }
}