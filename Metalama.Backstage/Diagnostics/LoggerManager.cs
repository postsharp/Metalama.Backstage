// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Infrastructure;
using Metalama.Backstage.Maintenance;
using System;
using System.Collections.Concurrent;

namespace Metalama.Backstage.Diagnostics;

internal class LoggerManager
{
    private readonly Func<LoggerManager, string, ILoggerFactory> _createLoggerFactory;
    private readonly ConcurrentDictionary<string, WeakReference<ILoggerFactory>> _loggerFactoriesByScope = new();

    public LoggerManager(
        IServiceProvider serviceProvider,
        DiagnosticsConfiguration configuration,
        ProcessKind processKind,
        Func<LoggerManager, string, ILoggerFactory> createLoggerFactory )
    {
        this._createLoggerFactory = createLoggerFactory;
        this.DateTimeProvider = serviceProvider.GetRequiredBackstageService<IDateTimeProvider>();

        this.Configuration = configuration;
        this.ProcessKind = processKind;

        var tempFileManager = serviceProvider.GetRequiredBackstageService<ITempFileManager>();

        if ( configuration.Logging.Processes.TryGetValue( processKind.ToString(), out var enabled ) && enabled )
        {
            this.LogDirectory = tempFileManager.GetTempDirectory( "Logs", CleanUpStrategy.Always );
        }
    }

    public string? LogDirectory { get; }

    internal DiagnosticsConfiguration Configuration { get; }

    internal IDateTimeProvider DateTimeProvider { get; }

    public ProcessKind ProcessKind { get; }

    public ILoggerFactory GetLoggerFactory( string scope )
    {
        if ( !this._loggerFactoriesByScope.TryGetValue( scope, out var weakRef ) )
        {
            weakRef = this._loggerFactoriesByScope.GetOrAdd( scope, this.CreateLoggerFactory );
        }

        if ( weakRef.TryGetTarget( out var loggerFactory ) )
        {
            return loggerFactory;
        }
        else
        {
            this._loggerFactoriesByScope.TryRemove( scope, out _ );

            return this.GetLoggerFactory( scope );
        }
    }

    private WeakReference<ILoggerFactory> CreateLoggerFactory( string s )
    {
        if ( this.LogDirectory != null )
        {
            return new WeakReference<ILoggerFactory>( this._createLoggerFactory( this, s ) );
        }
        else
        {
            return new WeakReference<ILoggerFactory>( NullLogger.Instance );
        }
    }

    public void RemoveLoggerFactory( ILoggerFactory loggerFactory )
    {
        this._loggerFactoriesByScope.TryRemove( loggerFactory.Scope, out _ );
    }
}