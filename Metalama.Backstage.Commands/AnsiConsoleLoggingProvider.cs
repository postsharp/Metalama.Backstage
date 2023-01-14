// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.Extensions.Logging;
using System;

namespace Metalama.Backstage.Commands;

internal class AnsiConsoleLoggingProvider : ILoggerProvider
{
    private readonly bool _verbose;
    private readonly ConsoleWriter _consoleWriter;

    public AnsiConsoleLoggingProvider( bool verbose, ConsoleWriter consoleWriter )
    {
        this._verbose = verbose;
        this._consoleWriter = consoleWriter;
    }

    public void Dispose() { }

    public ILogger CreateLogger( string categoryName ) => new Logger( this );

    private class Logger : ILogger
    {
        private readonly AnsiConsoleLoggingProvider _parent;

        public Logger( AnsiConsoleLoggingProvider parent )
        {
            this._parent = parent;
        }

        public void Log<TState>( LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter )
        {
            var message = formatter( state, exception );

            switch ( logLevel )
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    this._parent._consoleWriter.WriteError( message );

                    break;

                case LogLevel.Warning:
                    this._parent._consoleWriter.WriteWarning( message );

                    break;

                case LogLevel.Information:
                    this._parent._consoleWriter.WriteImportantMessage( message );

                    break;

                case LogLevel.Trace:
                case LogLevel.Debug:
                    this._parent._consoleWriter.WriteMessage( message );

                    break;
            }
        }

        public bool IsEnabled( LogLevel logLevel ) => logLevel >= LogLevel.Information || this._parent._verbose;

        public IDisposable? BeginScope<TState>( TState state )
            where TState : notnull
            => null;
    }
}