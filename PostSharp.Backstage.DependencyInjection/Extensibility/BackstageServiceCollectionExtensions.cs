// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.DependencyInjection.Extensibility
{
    public static class BackstageServiceCollectionExtensions
    {
        public static IServiceCollection AddServices( this IServiceCollection serviceCollection, IBackstageServiceCollection backstageServiceCollection )
        {
            foreach ( var service in backstageServiceCollection.EnumerateServices() )
            {
                serviceCollection.AddSingleton( service.Type, service.Instance );
            }

            return serviceCollection;
        }
    }
}