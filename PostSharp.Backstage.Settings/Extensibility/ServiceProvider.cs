// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Retrieves service objects; that is, an object that provides custom support to other objects.
    /// </summary>
    public class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        /// <summary>
        /// Sets the object implementing the service specified by <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">Type of service object to set.</typeparam>
        /// <param name="service">A service object of type <typeparamref name="T"/>.</param>
        public void SetService<T>( T service )
            where T : notnull
            => this._services.Add( typeof( T ), service );

        /// <inheritdoc />
        public object? GetService( Type serviceType )
        {
            _ = this._services.TryGetValue( serviceType, out var instance );

            return instance;
        }
    }
}