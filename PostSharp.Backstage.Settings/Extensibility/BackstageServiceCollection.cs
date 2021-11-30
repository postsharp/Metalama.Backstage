// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Extensibility
{
    public class BackstageServiceCollection
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly IServiceProvider? _nextServiceProvider;

        public BackstageServiceCollection( IServiceProvider? nextServiceProvider = null )
        {
            this._nextServiceProvider = nextServiceProvider;
        }

        public BackstageServiceCollection AddSingleton<TService>( TService service )
            where TService : notnull
        {
            this._services[typeof(TService)] = service;

            return this;
        }

        public BackstageServiceCollection AddSingleton<TService>( Func<BackstageServiceCollection, TService> serviceFactory )
            where TService : notnull
        {
            return this.AddSingleton( serviceFactory( this ) );
        }

        public IServiceProvider ToServiceProvider()
        {
            return new ServiceProvider( this );
        }

        private class ServiceProvider : IServiceProvider
        {
            private readonly BackstageServiceCollection _parent;

            public ServiceProvider( BackstageServiceCollection parent )
            {
                this._parent = parent;
            }

            object? IServiceProvider.GetService( Type serviceType )
            {
                return this.GetService( serviceType );
            }

            public object? GetService( Type serviceType )
            {
                if ( this._parent._services.TryGetValue( serviceType, out var service ) )
                {
                    return service;
                }
                else
                {
                    return this._parent._nextServiceProvider?.GetService( serviceType );
                }
            }
        }
    }
}