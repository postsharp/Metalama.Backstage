// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>( this IServiceProvider serviceProvider )
        {
            var service = (T?) serviceProvider.GetService( typeof(T) );

            if ( service == null )
            {
                throw new ServiceUnavailableException<T>();
            }

            return service;
        }
    }
}