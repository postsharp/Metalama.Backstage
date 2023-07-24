// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Extensibility
{
    /// <summary>
    /// Wraps a service provider factory or service collection, so that this project can
    /// register services in an arbitrary provider. 
    /// </summary>
    [PublicAPI]
    public sealed class ServiceProviderBuilder
    {
        private readonly Action<Type, object> _addService;
        private readonly Func<IServiceProvider> _getServiceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilder"/> class backed by an arbitrary implementation of <see cref="IServiceProvider"/>.
        /// </summary>
        public ServiceProviderBuilder( Action<Type, object> addService, Func<IServiceProvider> getServiceProvider )
        {
            this._addService = addService;
            this._getServiceProvider = getServiceProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderBuilder"/> class backed by a default implementation of <see cref="IServiceProvider"/>.
        /// </summary>
        public ServiceProviderBuilder()
        {
            var impl = new ServiceProviderImpl();
            this._addService = ( type, o ) => impl[type] = o;
            this._getServiceProvider = () => impl;
        }

        public void AddService( Type type, object instance ) => this._addService( type, instance );

        public IServiceProvider ServiceProvider => this._getServiceProvider();

        private class ServiceProviderImpl : Dictionary<Type, object>, IServiceProvider
        {
            object? IServiceProvider.GetService( Type serviceType )
            {
                if ( this.TryGetValue( serviceType, out var serviceValue ) )
                {
                    return serviceValue;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}