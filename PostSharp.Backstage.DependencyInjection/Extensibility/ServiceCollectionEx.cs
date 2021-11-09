// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.DependencyInjection.Extensibility
{
    public class ServiceCollectionEx : ServiceCollection, IServiceCollectionEx
    {
        private IServiceCollection AsServiceCollection() => this;

        IBackstageServiceCollection IBackstageServiceCollection.AddSingleton<TService>( IService service )
            where TService : class
        {
            this.AsServiceCollection().AddSingleton<TService>( (TService) service );

            return this;
        }

        IBackstageServiceCollection IBackstageServiceCollection.AddSingleton<TService>( Func<IServiceProvider, TService> serviceFactory )
            where TService : class
        {
            this.AsServiceCollection().AddSingleton<TService>( serviceFactory );

            return this;
        }

        public IEnumerable<(Type Type, IService Instance)> EnumerateServices() => throw new NotImplementedException();
    }
}