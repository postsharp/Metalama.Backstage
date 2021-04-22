// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility
{
    public sealed class ServiceUnavailableException<T> : Exception
        where T : class, IService
    {
        public ServiceUnavailableException()
            : base( $"Service '{typeof( T ).FullName}' is not available." )
        {
        }
    }
}
