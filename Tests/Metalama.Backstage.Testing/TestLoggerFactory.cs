// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Metalama.Backstage.Testing;

public class TestLoggerFactory : ILoggerFactory
{
    private readonly ConcurrentDictionary<string, ILogger> _loggers = new();
    private readonly List<Entry> _entries = new();
    private readonly ITestOutputHelper _testOutputHelper;

    public TestLoggerFactory( ITestOutputHelper testOutputHelper )
    {
        this._testOutputHelper = testOutputHelper;
    }

    [UsedImplicitly]
    public void Clear()
    {
        lock ( this._entries )
        {
            this._entries.Clear();
        }
    }

    public IReadOnlyList<Entry> Entries => this._entries;

    public void Dispose() { }

    public ILogger GetLogger( string category ) => this._loggers.GetOrAdd( category, c => new Logger( c, this ) );

    public void Flush() { }

    private class Logger : ILogger
    {
        private readonly string _category;
        private readonly TestLoggerFactory _parent;

        public Logger( string category, TestLoggerFactory parent )
        {
            this._category = category;
            this._parent = parent;
            this.Trace = new LogWriter( this._category, Severity.Trace, this._parent );
            this.Info = new LogWriter( this._category, Severity.Info, this._parent );
            this.Warning = new LogWriter( this._category, Severity.Warning, this._parent );
            this.Error = new LogWriter( this._category, Severity.Error, this._parent );
        }

        public ILogWriter? Trace { get; set; }

        public ILogWriter? Info { get; set; }

        public ILogWriter? Warning { get; set; }

        public ILogWriter? Error { get; set; }

        public ILogger WithPrefix( string prefix ) => this._parent.GetLogger( this._category + "." + prefix );
    }

    private class LogWriter : ILogWriter
    {
        private readonly string _category;
        private readonly Severity _severity;
        private readonly TestLoggerFactory _parent;

        public LogWriter( string category, Severity severity, TestLoggerFactory parent )
        {
            this._category = category;
            this._severity = severity;
            this._parent = parent;
        }

        public void Log( string message )
        {
            lock ( this._parent._entries )
            {
                this._parent._entries.Add( new Entry( this._severity, message ) );
            }

            this._parent._testOutputHelper.WriteLine( $"{this._severity.ToString().ToUpperInvariant()} {this._category}: {message}" );
        }
    }

    public enum Severity
    {
        Trace,
        Info,
        Warning,
        Error
    }

    public record Entry( Severity Severity, string Message );
}