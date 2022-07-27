// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Backstage.Extensibility
{
    // Intentionally internal because the names conflict with Microsoft.Extensions.DependencyInjection.
    internal static class ServiceProviderExtensions
    {
        public static TService? GetService<TService>( this IServiceProvider serviceProvider )
        {
            return (TService?) serviceProvider.GetService( typeof(TService) );
        }

        public static TService GetRequiredService<TService>( this IServiceProvider serviceProvider )
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