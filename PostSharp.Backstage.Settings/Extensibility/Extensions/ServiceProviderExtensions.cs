// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static TService? GetService<TService>( this IServiceProvider serviceProvider )
            where TService : class, IService
            => (TService?) serviceProvider.GetService( typeof(TService) );

        public static TService GetRequiredService<TService>( this IServiceProvider serviceProvider )
            where TService : class, IService
        {
            var service = serviceProvider.GetService<TService>();

            if ( service == null )
            {
                throw new InvalidOperationException( $"There is no service of type {typeof(TService).Name}" );
            }

            return service;
        }
    }
}