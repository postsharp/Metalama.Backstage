// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Diagnostics;

internal class Logger : ILogger
{
    public LoggerFactory LoggerFactory { get; }

    public string Category { get; }

    public string Prefix { get; }

    public Logger( LoggerFactory loggerDiagnosticsService, string category, string prefix = "" )
    {
        this.Prefix = prefix;
        this.LoggerFactory = loggerDiagnosticsService;
        this.Category = category;
        this.Error = this.CreateLogWriter( "ERROR" );
        this.Warning = this.CreateLogWriter( "WARNING" );
        this.Info = this.CreateLogWriter( "INFO" );

        if ( this.LoggerFactory.Configuration.Logging.IsTraceCategoryEnabled( category ) )
        {
            this.Trace = this.CreateLogWriter( "TRACE" );
        }
    }

    private LogWriter? CreateLogWriter( string logLevel ) => new( this, logLevel );

    public ILogWriter? Trace { get; }

    public ILogWriter? Info { get; }

    public ILogWriter? Warning { get; }

    public ILogWriter? Error { get; }

    public ILogger WithPrefix( string prefix )
        => new Logger( this.LoggerFactory, this.Category, string.IsNullOrEmpty( this.Prefix ) ? prefix : this.Prefix + " - " + prefix );
}