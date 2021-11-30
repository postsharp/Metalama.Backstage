using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Extensibility
{
    public class BackstageServiceCollection
    {
        private readonly Dictionary<Type, object> _services = new();
        private IServiceProvider? _nextServiceProvider;

        public BackstageServiceCollection( IServiceProvider? nextServiceProvider = null )
        {
            _nextServiceProvider = nextServiceProvider;
        }

        public BackstageServiceCollection AddSingleton<TService>( TService service )
        {
            _services[typeof(TService)] = service;

            return this;
        }

        public BackstageServiceCollection AddSingleton<TService>( Func<BackstageServiceCollection, TService> serviceFactory )
        {
            return AddSingleton<TService>( serviceFactory( this ) );
        }

        public IServiceProvider ToServiceProvider()
        {
            return new ServiceProvider( this );
        }

        private class ServiceProvider : IServiceProvider
        {
            private BackstageServiceCollection _parent;

            public ServiceProvider( BackstageServiceCollection parent )
            {
                _parent = parent;
            }

            object? IServiceProvider.GetService( Type serviceType )
            {
                return GetService( serviceType );
            }

            public object? GetService( Type serviceType )
            {
                if (_parent._services.TryGetValue( serviceType, out var service ))
                {
                    return service;
                }
                else
                {
                    return _parent._nextServiceProvider?.GetService( serviceType );
                }
            }
        }
    }
}