﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using IMicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IPostSharpLogger = Metalama.Backstage.Diagnostics.ILogger;
using IPostSharpLoggerFactory = Metalama.Backstage.Diagnostics.ILoggerFactory;

namespace Metalama.Backstage.MicrosoftLogging
{
    internal class LoggerFactoryAdapter : IPostSharpLoggerFactory
    {
        private readonly IMicrosoftLoggerFactory _factory;

        public LoggerFactoryAdapter( IMicrosoftLoggerFactory factory )
        {
            this._factory = factory;
        }

        public IPostSharpLogger GetLogger( string category ) => new LoggerAdapter( this._factory.CreateLogger( category ) );

        public void Dispose() { }
    }
}