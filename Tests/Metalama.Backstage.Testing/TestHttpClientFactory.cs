// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Infrastructure;
using System.Net.Http;

namespace Metalama.Backstage.Testing
{
    public class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpMessageHandler Handler { get; }

        public TestHttpClientFactory( HttpMessageHandler handler )
        {
            this.Handler = handler;
        }

        public HttpClient Create() => new( this.Handler );
    }
}