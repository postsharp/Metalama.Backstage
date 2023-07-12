// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NET

using Metalama.Backstage.Internal.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Backstage.Licensing.Tests.Telemetry;

internal class TelemetryTestsPutMessageHandler : HttpMessageHandler
{
    private readonly TelemetryPutHandler _telemetryPutHandler;

    public TelemetryTestsPutMessageHandler(IServiceProvider serviceProvider, string outputDirectory)
    {
        this._telemetryPutHandler = new TelemetryPutHandler( serviceProvider, outputDirectory );
    }

    protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage requestMessage, CancellationToken cancellationToken )
    {
        var request = await MessageToRequest( requestMessage );
        var response = await this._telemetryPutHandler.HandleAsync( request, cancellationToken );

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var httpContext = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };

        await response.ExecuteAsync( httpContext );
        var responseMessage = new HttpResponseMessage( (HttpStatusCode) httpContext.Response.StatusCode );
        return responseMessage;
    }

    // https://stackoverflow.com/a/68453301/4100001
    private static async Task<HttpRequest> MessageToRequest( HttpRequestMessage requestMessage )
    {
        if ( requestMessage.RequestUri == null )
        {
            throw new ArgumentException( $"{nameof( requestMessage )}.{nameof( requestMessage.RequestUri )} is null.", nameof( requestMessage ) );
        }

        if ( requestMessage.Content == null )
        {
            throw new ArgumentException( $"{nameof( requestMessage )}.{nameof( requestMessage.Content )} is null.", nameof( requestMessage ) );
        }

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = requestMessage.Method.Method;
        httpContext.Request.Scheme = requestMessage.RequestUri.Scheme;
        httpContext.Request.Host = new HostString( requestMessage.RequestUri.Host, requestMessage.RequestUri.Port );
        httpContext.Request.Path = requestMessage.RequestUri.AbsolutePath;
        httpContext.Request.QueryString = new QueryString( requestMessage.RequestUri.Query );

        foreach ( var header in requestMessage.Content.Headers )
        {
            httpContext.Request.Headers.Add( header.Key, new StringValues( header.Value.ToArray() ) );
        }

        var stream = new MemoryStream();
        await requestMessage.Content.CopyToAsync( stream );
        await stream.FlushAsync();
        stream.Position = 0;
        httpContext.Request.Body = stream;

        return httpContext.Request;
    }
}

#endif