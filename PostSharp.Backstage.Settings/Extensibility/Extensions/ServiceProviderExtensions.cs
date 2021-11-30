using System;

namespace PostSharp.Backstage.Extensibility.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static TService? GetService<TService>( this IServiceProvider serviceProvider )
        {
            return (TService?)serviceProvider.GetService( typeof(TService) );
        }

        public static TService GetRequiredService<TService>( this IServiceProvider serviceProvider )
        {
            var service = serviceProvider.GetService<TService>();

            if (service == null)
            {
                throw new InvalidOperationException( $"There is no service of type {typeof(TService).Name}" );
            }

            return service;
        }
    }
}