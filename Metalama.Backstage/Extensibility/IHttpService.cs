// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Net.Http;
using System.Threading.Tasks;

namespace Metalama.Backstage.Extensibility
{
    // TODO: Add missing methods.

    /// <summary>
    /// Provides sending HTTP requests and receiving HTTP responses
    /// from a resource identified by a URI.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Send a PUT request to the specified Uri as an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="content">The HTTP request content sent to the server.</param>
        Task<HttpResponseMessage> PutAsync( string requestUri, HttpContent content );
    }
}