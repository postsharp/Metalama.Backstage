// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Extension methods for setting up default services in an <see cref="IBackstageServiceCollection" />.
    /// </summary>
    public static class BackstageServiceServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a service providing current date and time using <see cref="DateTime.Now" /> to the specified <see cref="IBackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IBackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IBackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static IBackstageServiceCollection AddCurrentDateTimeProvider( this IBackstageServiceCollection serviceCollection )
            => serviceCollection.AddSingleton<IDateTimeProvider>( new CurrentDateTimeProvider() );

        /// <summary>
        /// Adds a service providing access to file system using API in <see cref="System.IO" /> namespace to the specified <see cref="IBackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IBackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IBackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static IBackstageServiceCollection AddFileSystem( this IBackstageServiceCollection serviceCollection )
            => serviceCollection.AddSingleton<IFileSystem>( new FileSystem() );

        /// <summary>
        /// Adds a service providing paths of standard directories to the specified <see cref="IBackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IBackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IBackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static IBackstageServiceCollection AddStandardDirectories( this IBackstageServiceCollection serviceCollection )
            => serviceCollection.AddSingleton<IStandardDirectories>( new StandardDirectories() );
    }
}