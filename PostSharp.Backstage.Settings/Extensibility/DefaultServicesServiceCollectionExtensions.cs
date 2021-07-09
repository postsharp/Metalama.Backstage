// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using System;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Extension methods for setting up default services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class DefaultServicesServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a service providing current date and time using <see cref="DateTime.Now" /> to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddCurrentDateTimeProvider( this IServiceCollection serviceCollection )
            => serviceCollection.AddSingleton<IDateTimeProvider>( new CurrentDateTimeProvider() );

        /// <summary>
        /// Adds a service providing access to file system using API in <see cref="System.IO" /> namespace to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddFileSystem( this IServiceCollection serviceCollection )
            => serviceCollection.AddSingleton<IFileSystem>( new FileSystem() );

        /// <summary>
        /// Adds a service providing paths of standard directories to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
        public static IServiceCollection AddStandardDirectories( this IServiceCollection serviceCollection )
            => serviceCollection.AddSingleton<IStandardDirectories>( new StandardDirectories() );
    }
}