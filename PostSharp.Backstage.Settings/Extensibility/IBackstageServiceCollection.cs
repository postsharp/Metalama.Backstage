using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Extensibility
{
    public interface IBackstageServiceCollection
    {
        IBackstageServiceCollection AddSingleton<TService>( IService service )
            where TService : class, IService;
        
        IBackstageServiceCollection AddSingleton<TService>( Func<IServiceProvider, TService> serviceFactory )
            where TService : class, IService;

        IEnumerable<(Type Type, IService Instance)> EnumerateServices();
    }
}