// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Provides extension methods to <see cref="IServiceProvider" />.
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of service object to set.</typeparam>
        /// <param name="serviceProvider">The service provider to retrieve the service from.</param>
        /// <returns>
        /// A service object of type <typeparamref name="T" />.
        /// </returns>
        /// <exception cref="ServiceUnavailableException{T}">
        /// Thrown when if there is no service object of type <typeparamref name="T"/>.
        /// </exception>
        public static T GetService<T>( this IServiceProvider serviceProvider )
        {
            var service = (T?) serviceProvider.GetService( typeof( T ) );

            if ( service == null )
            {
                throw new ServiceUnavailableException<T>();
            }

            return service;
        }
    }
}