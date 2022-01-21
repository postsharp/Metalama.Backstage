// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Logging;
using IMicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IPostSharpLogger = PostSharp.Backstage.Logging.ILogger;
using IPostSharpLoggerFactory = PostSharp.Backstage.Logging.ILoggerFactory;

namespace PostSharp.Backstage.MicrosoftLogging
{
    internal class LoggerFactoryAdapter : IPostSharpLoggerFactory
    {
        private readonly IMicrosoftLoggerFactory _factory;

        public LoggerFactoryAdapter( IMicrosoftLoggerFactory factory )
        {
            this._factory = factory;
        }

        public IPostSharpLogger CreateLogger<T>() where T : ILogCategory, new() =>
            new LoggerAdapter( this._factory.CreateLogger<T>() );
    }
}