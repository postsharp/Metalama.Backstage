// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace PostSharp.Backstage.Extensibility
{
    public class BackstageServiceProvider : IServiceProvider, IBackstageServiceCollection
    {
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        object? IServiceProvider.GetService( Type serviceType ) => this.GetService( serviceType );

        public IService? GetService( Type serviceType ) => this._services.TryGetValue( serviceType, out IService service ) ? service : null;

        public IBackstageServiceCollection AddSingleton<TService>( IService service )
            where TService : class, IService
        {
            this._services[typeof(TService)] = service;

            return this;
        }

        public IBackstageServiceCollection AddSingleton<TService>( Func<IServiceProvider, TService> serviceFactory )
            where TService : class, IService
            => this.AddSingleton<TService>( serviceFactory( this ) );

        public IEnumerable<(Type Type, IService Instance)> EnumerateServices() => this._services.Select( kv => (kv.Key, kv.Value) );
    }
}