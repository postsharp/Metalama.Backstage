// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Configuration;
using PostSharp.Backstage.Diagnostics;
using PostSharp.Backstage.Telemetry;
using System;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Extension methods for setting up default services in an <see cref="ServiceProviderBuilder" />.
    /// </summary>
    public static class RegisterServiceExtensions
    {
        public static ServiceProviderBuilder AddSingleton<T>(
            this ServiceProviderBuilder serviceProviderBuilder,
            T instance )
            where T : notnull
        {
            serviceProviderBuilder.AddService( typeof(T), instance );

            return serviceProviderBuilder;
        }

        /// <summary>
        /// Adds a service providing current date and time using <see cref="DateTime.Now" /> to the specified <see cref="ServiceProviderBuilder" />.
        /// </summary>
        /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
        /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
        private static ServiceProviderBuilder AddCurrentDateTimeProvider( this ServiceProviderBuilder serviceProviderBuilder )
            => serviceProviderBuilder.AddSingleton<IDateTimeProvider>( new CurrentDateTimeProvider() );

        /// <summary>
        /// Adds a service providing access to file system using API in <see cref="System.IO" /> namespace to the specified <see cref="ServiceProviderBuilder" />.
        /// </summary>
        /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
        /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
        private static ServiceProviderBuilder AddFileSystem( this ServiceProviderBuilder serviceProviderBuilder )
            => serviceProviderBuilder.AddSingleton<IFileSystem>( new FileSystem() );

        /// <summary>
        /// Adds a service providing paths of standard directories to the specified <see cref="ServiceProviderBuilder" />.
        /// </summary>
        /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
        /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
        internal static ServiceProviderBuilder AddStandardDirectories( this ServiceProviderBuilder serviceProviderBuilder )
            => serviceProviderBuilder.AddSingleton<IStandardDirectories>( new StandardDirectories() );

        private static ServiceProviderBuilder AddDiagnostics( this ServiceProviderBuilder serviceProviderBuilder, ProcessKind processKind )
        {
            var serviceProvider = serviceProviderBuilder.ServiceProvider;
            var service = DiagnosticsService.GetInstance( serviceProvider, processKind );

            return serviceProviderBuilder.AddSingleton<ILoggerFactory>( service ).AddSingleton<IDebuggerService>( service );
        }

        /// <summary>
        /// Adds the minimal set of services required to run the <see cref="DiagnosticsService"/>.
        /// </summary>
        public static ServiceProviderBuilder AddMinimalServices( this ServiceProviderBuilder serviceProviderBuilder )
        {
            return serviceProviderBuilder.AddCurrentDateTimeProvider()
                .AddFileSystem()
                .AddStandardDirectories();
        }

        public static ServiceProviderBuilder AddConfigurationManager( this ServiceProviderBuilder serviceProviderBuilder )
        {
            return
                serviceProviderBuilder.AddSingleton<IConfigurationManager>( new ConfigurationManager( serviceProviderBuilder.ServiceProvider ) );
        }

        public static ServiceProviderBuilder AddBackstageServices( this ServiceProviderBuilder serviceProviderBuilder, IApplicationInfo? applicationInfo )
        {
            if ( applicationInfo != null )
            {
                serviceProviderBuilder = serviceProviderBuilder.AddSingleton( applicationInfo );
            }

            serviceProviderBuilder = serviceProviderBuilder
                .AddCurrentDateTimeProvider()
                .AddFileSystem()
                .AddStandardDirectories();

            if ( applicationInfo != null && applicationInfo.ProcessKind != ProcessKind.Other )
            {
                serviceProviderBuilder = serviceProviderBuilder.AddDiagnostics( applicationInfo.ProcessKind );
            }

            serviceProviderBuilder =
                serviceProviderBuilder.AddConfigurationManager();

            var uploadManager = new UploadManager( serviceProviderBuilder.ServiceProvider );

            serviceProviderBuilder = serviceProviderBuilder
                .AddSingleton<IExceptionReporter>( new ExceptionReporter( uploadManager, serviceProviderBuilder.ServiceProvider ) )
                .AddSingleton<IUsageReporter>( new UsageReporter( uploadManager, serviceProviderBuilder.ServiceProvider ) );

            return serviceProviderBuilder;
        }
    }
}