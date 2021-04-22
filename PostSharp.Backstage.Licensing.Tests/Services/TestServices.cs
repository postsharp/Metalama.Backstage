// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal class TestServices : IServiceLocator
    {
        private readonly Dictionary<Type, IService> _services = new();

        public TestServices()
        {
            this._services.Add( typeof( IApplicationInfoService ), new ApplicationInfoService( false, new( 0, 1, 0 ), new( 2021, 1, 1 ) ) );
            this._services.Add( typeof( IDateTimeProvider ), new CurrentDateTimeProvider() );
        }

        public bool TryGetService<T>( [MaybeNullWhen( returnValue: false )] out T service )
            where T : class, IService
        {
            if ( this._services.TryGetValue( typeof( T ), out var s ) )
            {
                service = (T) s;
                return true;
            }
            else
            {
                service = null;
                return false;
            }
        }

        public T GetService<T>()
            where T : class, IService
        {
            if ( this.TryGetService<T>( out var service ) )
            {
                return service;
            }
            else
            {
                throw new ServiceUnavailableException<T>();
            }
        }
    }
}
