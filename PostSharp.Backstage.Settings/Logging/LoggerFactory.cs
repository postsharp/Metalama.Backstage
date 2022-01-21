// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Utilities;
using System;
using System.Diagnostics;
using System.IO;

namespace PostSharp.Backstage.Logging;

public class LoggerFactory : ILoggerFactory
{
    public TextWriter? TextWriter { get; }

    public LoggingConfiguration Configuration { get; }

    public LoggerFactory( IServiceProvider serviceProvider, LoggingProcessKind processKind )
    {
        this.Configuration = LoggingConfiguration.Load( serviceProvider );

        if ( this.Configuration.Processes.TryGetValue( processKind, out var enabled ) && enabled )
        {
            var pid = Process.GetCurrentProcess().Id;

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

                // The filename must be unique because several instances of the current assembly (of different versions) may be loaded in the process.
                this.TextWriter = File.CreateText(
                    Path.Combine(
                        directory,
                        $"Metalama.{Process.GetCurrentProcess().ProcessName}.{pid}.{Guid.NewGuid()}.log" ) );
            }
            catch
            {
                // Don't fail if we cannot initialize the log.
            }
        }
    }

    public ILogger CreateLogger<T>()
        where T : ILogCategory, new()
        => this.TextWriter != null ? new Logger( this, new T().Name ) : NullLogger.Instance;
}