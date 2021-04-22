// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics.CodeAnalysis;

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Exposes compile-time services.
    /// </summary>
    public interface IServiceLocator : IService
    {
        /// <summary>
        /// Gets a compile-time service.
        /// </summary>
        /// <typeparam name="T">An interface derived from <see cref="IService"/>.</typeparam>
        /// <param name="throwing"><c>true</c> whether an exception should be thrown in case the service cannot be acquired, otherwise <c>false</c>.
        /// The default value is <c>true</c>.</param>
        /// <returns>The service <typeparamref name="T"/>, or <c>null</c> if the service could not be acquired and <paramref name="throwing"/>
        /// was set to <c>false</c>.</returns>
        bool TryGetService<T>( [MaybeNullWhen( returnValue: false )] out T service )
            where T : class, IService;

        T GetService<T>()
            where T : class, IService;
    }
}
