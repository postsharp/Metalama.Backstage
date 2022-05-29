// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Net.Http;
using System.Threading.Tasks;

namespace Metalama.Backstage.Extensibility
{
    // This service can be improved by adding more methods and managing
    // the HttpClient instace in a diferent way.
    // At the moment, it serves the current requirements.

    /// <inheritdoc />
    internal class HttpService : IHttpService
    {
        /// <inheritdoc />
        public Task<HttpResponseMessage> PutAsync( string requestUri, HttpContent content )
        {
            using var client = new HttpClient();
            return client.PostAsync( requestUri, content );
        }
    }
}