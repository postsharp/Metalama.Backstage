// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;

namespace PostSharp.Backstage.Extensibility
{
    public class DictionaryServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, IService> _services = new Dictionary<Type, IService>();

        object? IServiceProvider.GetService( Type serviceType ) => this.GetService( serviceType );

        public IService? GetService( Type serviceType ) => this._services.TryGetValue( serviceType, out IService service ) ? service : null;

        public void AddService<TService>( IService service )
            where TService : IService
            => this._services[typeof(TService)] = service;

        public IEnumerable<(Type ServiceType, IService ServiceImplementation)> EnumerateServices() 
            => this._services.Select( kv => (kv.Key, kv.Value) );
    }
}