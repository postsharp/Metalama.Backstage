// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Thrown when a requested service object is not available.
    /// </summary>
    /// <typeparam name="T">Type of the requested service.</typeparam>
    public sealed class ServiceUnavailableException<T> : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceUnavailableException{T}"/> class.
        /// </summary>
        public ServiceUnavailableException()
            : base( $"Service '{typeof( T ).FullName}' is not available." )
        {
        }
    }
}
