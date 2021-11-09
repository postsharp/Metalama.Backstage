// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using MELT;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.DependencyInjection.Extensibility;
using PostSharp.Backstage.DependencyInjection.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Testing.Services;
using System;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Testing
{
    public abstract class TestsBase
    {
        public TestDateTimeProvider Time { get; } = new();

        public TestFileSystem FileSystem { get; } = new();

        public ITestLoggerSink Log { get; }

        public IServiceProvider Services { get; }

        public TestsBase( ITestOutputHelper logger, Action<IServiceCollectionEx>? serviceBuilder = null )
        {
            var loggerFactory = TestLoggerFactory
                .Create()
                .AddXUnit( logger );

            this.Log = loggerFactory.GetTestLoggerSink();

            // ReSharper disable RedundantTypeArgumentsOfMethod

            var serviceCollection = new ServiceCollectionEx();
                
            serviceCollection
                .AddMicrosoftLoggerFactory( loggerFactory )
                .AddSingleton<IDateTimeProvider>( this.Time )
                .AddSingleton<IFileSystem>( this.FileSystem );
                
            serviceCollection
                .AddStandardDirectories();

            // ReSharper restore RedundantTypeArgumentsOfMethod

            serviceBuilder?.Invoke( serviceCollection );

            this.Services = serviceCollection.BuildServiceProvider();
        }
    }
}