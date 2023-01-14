// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;

namespace Metalama.Backstage.Diagnostics;

internal class JsonTraceWriter : ITraceWriter
{
    private readonly ILogger _logger;

    public JsonTraceWriter( ILogger logger )
    {
        this._logger = logger;
    }

    public void Trace( TraceLevel level, string message, Exception? ex )
    {
        switch ( level )
        {
            case TraceLevel.Error:
                this._logger.Error?.Log( message );

                break;

            case TraceLevel.Warning:
                this._logger.Warning?.Log( message );

                break;

            case TraceLevel.Info:
                this._logger.Info?.Log( message );

                break;
            
            case TraceLevel.Verbose:
                this._logger.Trace?.Log( message );

                break;
        }
    }

    public TraceLevel LevelFilter => this._logger.Trace == null ? TraceLevel.Warning : TraceLevel.Verbose;
}