// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using MELT;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.MicrosoftLogging;
using Metalama.Backstage.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Xunit.Abstractions;

namespace Metalama.Backstage.Testing
{
    public abstract class TestsBase
    {
        private readonly TestProcessService _processService = new();

        private readonly TestHttpService _httpService = new();

        public TestDateTimeProvider Time { get; } = new();

        public TestFileSystem FileSystem { get; } = new();

        public ITestLoggerSink Log { get; }

        public IServiceProvider ServiceProvider { get; }

        public IReadOnlyList<ProcessStartInfo> StartedProcesses => this._processService.StartedProcesses;

        public IReadOnlyList<(HttpMethod Method, string Uri, HttpContent Content)> ReceivedHttpContent => this._httpService.ReceivedContent;

        public TestsBase( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
        {
            var loggerFactory = TestLoggerFactory
                .Create()
                .AddXUnit( logger );

            this.Log = loggerFactory.GetTestLoggerSink();

            // ReSharper disable RedundantTypeArgumentsOfMethod

            var serviceCollection = new ServiceCollection()
                .AddSingleton<IDateTimeProvider>( this.Time )
                .AddSingleton<IFileSystem>( this.FileSystem )
                .AddSingleton<IPlatformInfo>( new TestPlatformInfo() )
                .AddSingleton<IProcessService>( this._processService )
                .AddSingleton<IHttpService>( this._httpService );

            var serviceCollectionAdapter =
                new ServiceProviderBuilder(
                    ( type, instance ) => serviceCollection.AddSingleton( type, instance ),
                    () => serviceCollection.BuildServiceProvider() );

            serviceCollectionAdapter
                .AddMicrosoftLoggerFactory( loggerFactory )
                .AddStandardDirectories();

            // ReSharper restore RedundantTypeArgumentsOfMethod

            serviceBuilder?.Invoke( serviceCollectionAdapter );

            this.ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}