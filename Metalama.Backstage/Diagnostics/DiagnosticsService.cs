// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Configuration;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Utilities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace PostSharp.Backstage.Diagnostics;

public class DiagnosticsService : ILoggerFactory
{
    private static readonly object _initializeSync = new();
    private static readonly object _attachDebuggerSync = new();
    private static readonly DiagnosticsService _uninitializedInstance = new();
    private static volatile DiagnosticsService? _instance;
    private static volatile bool _attachDebuggerRequested;

    private readonly ProcessKind _processKind;
    private readonly ConcurrentDictionary<string, ILogger> _loggers = new( StringComparer.OrdinalIgnoreCase );
    private readonly object _textWriterSync = new();
    private readonly string? _fileName;
    private TextWriter? _textWriter;

    internal DiagnosticsConfiguration Configuration { get; }

    /// <summary>
    /// Gets the global instance of the <see cref="DiagnosticsService"/> class. If the <see cref="Initialize"/> method has not been
    /// called, it returns an uninitialized instance, which does not do anything. This property allows to access the service from parts
    /// of code that do not have access to a service provider.
    /// </summary>
    public static DiagnosticsService Instance => _instance ?? _uninitializedInstance;

    /// <summary>
    /// Initializes the global <see cref="Instance"/> of the <see cref="DiagnosticsService"/> class.
    /// </summary>
    /// <param name="processKind"></param>
    public static void Initialize( ProcessKind processKind, string? projectName = null )
    {
        lock ( _initializeSync )
        {
            if ( _instance != null )
            {
                var serviceProvider = new ServiceProviderBuilder().AddDiagnosticServiceRequirements();
                _instance = new DiagnosticsService( serviceProvider.ServiceProvider, processKind, projectName );
            }
        }
    }

    /// <summary>
    /// Gets a initialized instance of the <see cref="DiagnosticsService"/> class. If the <see cref="Initialize"/> method has been called, it
    /// returns the instance created by this method. Otherwise, it creates a new instance for the given <see cref="IServiceProvider"/>.
    /// </summary>
    public static DiagnosticsService GetInstance( IServiceProvider serviceProvider, ProcessKind processKind, string? projectName = null )
    {
        lock ( _initializeSync )
        {
            if ( _instance != null )
            {
                if ( _instance._processKind != processKind )
                {
                    throw new InvalidOperationException( "The class has been initialized with a different ProcessKind." );
                }

                return _instance;
            }
            else
            {
                return new DiagnosticsService( serviceProvider, processKind, projectName );
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsService"/> class that is returned by the <see cref="Instance"/> property before the
    /// <see cref="Initialize"/> method is called (or if it is never called).
    /// </summary>
    private DiagnosticsService()
    {
        this.Configuration = new DiagnosticsConfiguration();
    }

    private DiagnosticsService( IServiceProvider serviceProvider, ProcessKind processKind, string? projectName )
    {
        this._processKind = processKind;
        this.Configuration = serviceProvider.GetRequiredService<IConfigurationManager>().Get<DiagnosticsConfiguration>();

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
                    $"Metalama-{Process.GetCurrentProcess().ProcessName}{projectNameWithDot}-{Guid.NewGuid()}.log" );
            }
            catch
            {
                // Don't fail if we cannot initialize the log.
            }
        }

        this.LaunchDebugger();
    }

    public void WriteLine( string s )
    {
        lock ( this._textWriterSync )
        {
            this._textWriter ??= File.CreateText( this._fileName );

            this._textWriter.WriteLine( s );
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

    private void LaunchDebugger()
    {
        if ( this.Configuration.Debugger.Processes.TryGetValue( this._processKind, out var enabled ) && enabled )
        {
            lock ( _attachDebuggerSync )
            {
                if ( !_attachDebuggerRequested )
                {
                    // We try to request to attach the debugger a single time, even if the user refuses or if the debugger gets
                    // detached. It makes a better debugging experience.
                    _attachDebuggerRequested = true;

                    if ( !Debugger.IsAttached )
                    {
                        Debugger.Launch();
                    }
                }
            }
        }
    }

    public void Dispose() => this._textWriter?.Close();
}