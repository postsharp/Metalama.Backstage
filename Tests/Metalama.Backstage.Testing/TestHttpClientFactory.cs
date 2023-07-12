// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System.Net.Http;

namespace Metalama.Backstage.Testing
{
    public class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public TestHttpClientFactory( HttpMessageHandler handler )
        {
            this._handler = handler;
        }

        public HttpClient Create() => new HttpClient( this._handler );
    }
}