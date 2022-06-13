// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Metalama.Backstage.Diagnostics
{
    internal class LoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<string, ILogger> _loggers = new( StringComparer.OrdinalIgnoreCase );
        private readonly object _textWriterSync = new();
        private readonly string? _fileName;
        private TextWriter? _textWriter;

        internal DiagnosticsConfiguration Configuration { get; }

        public LoggerFactory( DiagnosticsConfiguration configuration, ProcessKind processKind, string? projectName )
        {
            this.Configuration = configuration;

            if ( this.Configuration.Logging.Processes.TryGetValue( processKind, out var enabled ) && enabled )
            {
                var directory = Path.Combine( Path.GetTempPath(), "Metalama", "Logs" );

                try
                {
                    RetryHelper.Retry(
                        () =>
                        {
                            if ( !Directory.Exists( directory ) )
                            {
                                Directory.CreateDirectory( directory );
                            }
                        } );

                    var projectNameWithDot = string.IsNullOrEmpty( projectName ) ? "" : "-" + projectName;

                    // The filename must be unique because several instances of the current assembly (of different versions) may be loaded in the process.
                    this._fileName = Path.Combine(
                        directory,
                        $"Metalama-{processKind}{projectNameWithDot}-{Guid.NewGuid()}.log" );
                }
                catch
                {
                    // Don't fail if we cannot initialize the log.
                }
            }
        }

        public void WriteLine( string s )
        {
            lock ( this._textWriterSync )
            {
                if ( this._fileName == null )
                {
                    throw new InvalidOperationException( $"Cannot write diagnostics. The '{this.GetType().Name}' is not properly initialized." );
                }

                this._textWriter ??= File.CreateText( this._fileName );

                this._textWriter.WriteLine( s );
                this._textWriter.Flush();
            }
        }

        public ILogger GetLogger( string category )
        {
            if ( this._fileName != null )
            {
                if ( this._loggers.TryGetValue( category, out var logger ) )
                {
                    return logger;
                }
                else
                {
                    logger = new Logger( this, category );

                    return this._loggers.GetOrAdd( category, logger );
                }
            }
            else
            {
                return NullLogger.Instance;
            }
        }

        public void Dispose() => this._textWriter?.Close();
    }
}