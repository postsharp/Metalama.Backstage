// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Diagnostics;
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
        internal static ServiceProviderBuilder AddCurrentDateTimeProvider( this ServiceProviderBuilder serviceProviderBuilder )
            => serviceProviderBuilder.AddSingleton<IDateTimeProvider>( new CurrentDateTimeProvider() );

        /// <summary>
        /// Adds a service providing access to file system using API in <see cref="System.IO" /> namespace to the specified <see cref="ServiceProviderBuilder" />.
        /// </summary>
        /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
        /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
        internal static ServiceProviderBuilder AddFileSystem( this ServiceProviderBuilder serviceProviderBuilder )
            => serviceProviderBuilder.AddSingleton<IFileSystem>( new FileSystem() );

        /// <summary>
        /// Adds a service providing paths of standard directories to the specified <see cref="ServiceProviderBuilder" />.
        /// </summary>
        /// <param name="serviceProviderBuilder">The <see cref="ServiceProviderBuilder" /> to add services to.</param>
        /// <returns>The <see cref="ServiceProviderBuilder" /> so that additional calls can be chained.</returns>
        internal static ServiceProviderBuilder AddStandardDirectories( this ServiceProviderBuilder serviceProviderBuilder )
            => serviceProviderBuilder.AddSingleton<IStandardDirectories>( new StandardDirectories() );

        public static ServiceProviderBuilder AddDiagnostics( this ServiceProviderBuilder serviceProviderBuilder, ProcessKind processKind )
        {
            var service = DiagnosticsService.GetInstance( serviceProviderBuilder.ServiceProvider, processKind );

            return serviceProviderBuilder.AddSingleton<ILoggerFactory>( service ).AddSingleton<IDebuggerService>( service );
        }

        public static ServiceProviderBuilder AddSystemServices( this ServiceProviderBuilder serviceProviderBuilder, ProcessKind processKind = ProcessKind.Other )
        {
            serviceProviderBuilder = serviceProviderBuilder.AddCurrentDateTimeProvider()
                .AddFileSystem()
                .AddStandardDirectories();

            if ( processKind != ProcessKind.Other )
            {
                serviceProviderBuilder = serviceProviderBuilder.AddDiagnostics( processKind );
            }

            return serviceProviderBuilder;
        }
    }
}