// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
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

        public LoggerFactory( IServiceProvider serviceProvider, DiagnosticsConfiguration configuration, ProcessKind processKind, string? projectName )
        {
            var tempFileManager = serviceProvider.GetRequiredBackstageService<ITempFileManager>();

            this.Configuration = configuration;

            if ( this.Configuration.Logging.Processes.TryGetValue( processKind, out var enabled ) && enabled )
            {
                var directory = tempFileManager.GetTempDirectory( "Logs", CleanUpStrategy.Always );

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