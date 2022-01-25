// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Diagnostics;

public class NullLogger : ILogger, ILoggerFactory
{
    private NullLogger() { }

    public static NullLogger Instance { get; } = new();

    ILogWriter? ILogger.Trace => null;

    ILogWriter? ILogger.Info => null;

    ILogWriter? ILogger.Warning => null;

    ILogWriter? ILogger.Error => null;

    ILogger ILoggerFactory.GetLogger( string category ) => this;

    public void Dispose() { }
}