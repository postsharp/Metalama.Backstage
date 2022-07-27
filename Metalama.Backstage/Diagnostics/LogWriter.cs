// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;
using System.Threading;

namespace Metalama.Backstage.Diagnostics;

internal class LogWriter : ILogWriter
{
    private readonly Logger _logger;
    private readonly string _logLevel;

    public LogWriter( Logger logger, string logLevel )
    {
        this._logger = logger;
        this._logLevel = logLevel.ToUpperInvariant();
    }

    public void Log( string message )
    {
        this._logger.LoggerFactory.WriteLine(
            FormattableString.Invariant(
                $"{DateTime.Now}, {this._logLevel}, {this._logger.Category}, Thread {Thread.CurrentThread.ManagedThreadId}: {message}" ) );
    }
}