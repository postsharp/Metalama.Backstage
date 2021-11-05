// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using System;

namespace PostSharp.Backstage.DependencyInjection.Logging
{
    public static class DefaultServicesServiceCollectionExtensions
    {
        private static readonly IServiceProvider _defaultServices = new DefaultServiceProvider();

        public static IServiceCollection AddStandardService<TService>( this IServiceCollection serviceCollection )
            where TService : class
            => serviceCollection.AddSingleton<TService>( _defaultServices.GetRequiredService<TService>() );
    }
}