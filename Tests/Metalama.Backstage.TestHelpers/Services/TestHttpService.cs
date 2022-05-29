// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Metalama.Backstage.Testing.Services
{
    // TODO: Implementation can be improved for more testing options.

    public class TestHttpService : IHttpService
    {
        private readonly List<(HttpMethod, string, HttpContent)> _receivedContent = new();

        public IReadOnlyList<(HttpMethod Method, string Uri, HttpContent Content)> ReceivedContent => this._receivedContent;

        public Task<HttpResponseMessage> PutAsync( string requestUri, HttpContent content )
        {
            this._receivedContent.Add( (HttpMethod.Put, requestUri, content) );
            return Task.FromResult( new HttpResponseMessage( HttpStatusCode.OK ) );
        }
    }
}