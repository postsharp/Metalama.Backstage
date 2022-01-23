// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Diagnostics;

internal class LogWriter : ILogWriter
{
    private readonly Logger _logger;
    private readonly string _logLevel;

    public LogWriter( Logger logger, string logLevel )
    {
        this._logger = logger;
        this._logLevel = logLevel;
    }

    public void Log( string message )
    {
        // ReSharper disable once InconsistentlySynchronizedField
        var writer = this._logger.DiagnosticsService.TextWriter;

        if ( writer != null )
        {
            lock ( writer )
            {
                writer.WriteLine(
                    FormattableString.Invariant( $"{DateTime.Now} {this._logLevel.ToString().ToUpperInvariant()} {this._logger.Category}: {message}" ) );
            }
        }
    }
}