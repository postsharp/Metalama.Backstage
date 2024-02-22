// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;

namespace Metalama.Backstage.Commands;

internal class AnsiConsoleLoggerFactory : ILoggerFactory, ILogger
{
    public AnsiConsoleLoggerFactory( ConsoleWriter consoleWriter, BaseCommandSettings settings )
    {
        this.Error = new LogWriter( consoleWriter.WriteError );

        if ( !settings.NoWarn )
        {
            this.Warning = new LogWriter( consoleWriter.WriteWarning );
        }

        this.Info = new LogWriter( consoleWriter.WriteImportantMessage );

        if ( settings.Verbose )
        {
            this.Trace = new LogWriter( consoleWriter.WriteMessage );
        }
    }

    public void Dispose() { }

    public ILogger GetLogger( string category ) => this;

    void ILoggerFactory.Flush() { }

    private class LogWriter : ILogWriter
    {
        private readonly Action<string> _action;

        public LogWriter( Action<string> action )
        {
            this._action = action;
        }

        public void Log( string message ) => this._action( message );
    }

    public ILogWriter? Trace { get; }

    public ILogWriter? Info { get; }

    public ILogWriter? Warning { get; }

    public ILogWriter? Error { get; }

    public ILogger WithPrefix( string prefix ) => this;
}