﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

namespace Metalama.Backstage.Diagnostics;

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