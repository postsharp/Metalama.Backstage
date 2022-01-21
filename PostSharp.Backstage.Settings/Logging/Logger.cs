// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Logging;

internal class Logger : ILogger
{
    public LoggerFactory LoggerFactory { get; }
    public string Category { get; }

    public Logger( LoggerFactory loggerLoggerFactory, string category )
    {
        this.LoggerFactory = loggerLoggerFactory;
        this.Category = category;
        this.Error = this.CreateLogWriter( "ERROR" );
        this.Warning = this.CreateLogWriter( "WARNING" );
        this.Info = this.CreateLogWriter( "INFO" );
        if ( (this.LoggerFactory.Configuration.Categories.TryGetValue( "*", out var allEnabled ) && allEnabled) ||
             (this.LoggerFactory.Configuration.Categories.TryGetValue( category, out var enabled ) && enabled) )
        {
            this.Trace = this.CreateLogWriter( "TRACE" );
        }
    }

    private LogWriter? CreateLogWriter( string logLevel ) => new(this, logLevel);


    public ILogWriter? Trace { get; }
    public ILogWriter? Info { get; }
    public ILogWriter? Warning { get; }
    public ILogWriter? Error { get; }
}