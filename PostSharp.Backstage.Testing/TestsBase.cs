// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using MELT;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Testing.Services;
using Xunit.Abstractions;

namespace PostSharp.Backstage.Testing
{
    public abstract class TestsBase
    {
        public TestDateTimeProvider Time { get; } = new();

        public TestFileSystem FileSystem { get; } = new();

        public ILoggerFactory LoggerFactory { get; }

        public ITestLoggerSink Log { get; }

        public IServiceProvider Services { get; }

        public TestsBase( ITestOutputHelper logger )
        {
            this.LoggerFactory = TestLoggerFactory
                .Create()
                .AddXUnit( logger );

            this.Log = this.LoggerFactory.GetTestLoggerSink();

            var serviceCollection = new ServiceCollection()
                .AddSingleton<ILoggerFactory>( this.LoggerFactory )
                .AddSingleton<IDateTimeProvider>( this.Time )
                .AddSingleton<IFileSystem>( this.FileSystem );

            this.SetUpServices( serviceCollection );
            this.Services = serviceCollection.BuildServiceProvider();
        }

        protected virtual IServiceCollection SetUpServices( IServiceCollection serviceCollection )
        {
            return serviceCollection;
        }
    }
}
