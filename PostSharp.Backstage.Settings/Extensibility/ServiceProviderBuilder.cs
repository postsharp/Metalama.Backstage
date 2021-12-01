// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Wraps a service provider factory or service collection, so that this project can
    /// register services in an arbitrary provider. 
    /// </summary>
    public sealed class ServiceProviderBuilder
    {
        private readonly Action<Type,object> _addService;
        private readonly Func<IServiceProvider> _getServiceProvider;

        public ServiceProviderBuilder( Action<Type, object> addService, Func<IServiceProvider> getServiceProvider )
        {
            this._addService = addService;
            this._getServiceProvider = getServiceProvider;
        }

        public void AddService( Type type, object instance ) => this._addService( type, instance );
        
        public IServiceProvider ServiceProvider => this._getServiceProvider();
    }

}