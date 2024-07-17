// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Infrastructure;
using System;
using System.Threading;

namespace Metalama.Backstage.Diagnostics;

internal class LogWriter : ILogWriter
{
    private readonly Logger _logger;
    private readonly string _logLevel;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LogWriter( Logger logger, string logLevel )
    {
        this._logger = logger;
        this._dateTimeProvider = logger.LoggerFactory.DateTimeProvider;
        this._logLevel = logLevel.ToUpperInvariant();
    }

    public void Log( string message )
    {
        if ( string.IsNullOrEmpty( this._logger.Prefix ) )
        {
            this._logger.LoggerFactory.WriteLine(
                FormattableString.Invariant(
                    $"{this._dateTimeProvider.UtcNow}, {this._logLevel}, Thread {Thread.CurrentThread.ManagedThreadId}, {this._logger.Category}: {message}" ) );
        }
        else
        {
            this._logger.LoggerFactory.WriteLine(
                FormattableString.Invariant(
                    $"{this._dateTimeProvider.UtcNow}, {this._logLevel},  Thread {Thread.CurrentThread.ManagedThreadId}, {this._logger.Category} - {this._logger.Prefix}: {message}" ) );
        }
    }
}