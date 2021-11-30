// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Extension methods for setting up default services in an <see cref="BackstageServiceCollection" />.
    /// </summary>
    public static class BackstageServiceServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a service providing current date and time using <see cref="DateTime.Now" /> to the specified <see cref="BackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="BackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="BackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static BackstageServiceCollection AddCurrentDateTimeProvider( this BackstageServiceCollection serviceCollection )
        {
            return serviceCollection.AddSingleton<IDateTimeProvider>( new CurrentDateTimeProvider() );
        }

        /// <summary>
        /// Adds a service providing access to file system using API in <see cref="System.IO" /> namespace to the specified <see cref="BackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="BackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="BackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static BackstageServiceCollection AddFileSystem( this BackstageServiceCollection serviceCollection )
        {
            return serviceCollection.AddSingleton<IFileSystem>( new FileSystem() );
        }

        /// <summary>
        /// Adds a service providing paths of standard directories to the specified <see cref="BackstageServiceCollection" />.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="BackstageServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="BackstageServiceCollection" /> so that additional calls can be chained.</returns>
        public static BackstageServiceCollection AddStandardDirectories( this BackstageServiceCollection serviceCollection )
        {
            return serviceCollection.AddSingleton<IStandardDirectories>( new StandardDirectories() );
        }
    }
}