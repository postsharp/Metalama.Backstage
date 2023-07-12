// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#if NET
using Metalama.Backstage.Internal.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Linq;
#endif

namespace Metalama.Backstage.Tests.Telemetry;

internal class TelemetryTestsPutMessageHandler : HttpMessageHandler
{
    public List<(HttpRequestMessage Request, HttpResponseMessage Response)> ProcessedRequests { get; } = new();

#if NET
    private readonly TelemetryPutHandler _telemetryPutHandler;
#endif

    public TelemetryTestsPutMessageHandler(IServiceProvider serviceProvider, string outputDirectory)
    {
#if NET
        this._telemetryPutHandler = new TelemetryPutHandler( serviceProvider, outputDirectory );
#endif
    }

    protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage requestMessage, CancellationToken cancellationToken )
    {
        HttpResponseMessage responseMessage;

#if NET
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
        responseMessage = new HttpResponseMessage( (HttpStatusCode) httpContext.Response.StatusCode );
#else
        // Get rid of "this method lacks await" warning.
        await Task.CompletedTask;

        responseMessage = new HttpResponseMessage( HttpStatusCode.OK );
#endif

        this.ProcessedRequests.Add( (requestMessage, responseMessage) );

        return responseMessage;
    }

#if NET
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
#endif
}