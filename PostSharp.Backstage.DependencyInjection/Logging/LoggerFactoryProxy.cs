// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.Logging;
using IMicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IPostSharpLogger = PostSharp.Backstage.Extensibility.ILogger;
using IPostSharpLoggerFactory = PostSharp.Backstage.Extensibility.ILoggerFactory;

namespace PostSharp.Backstage.DependencyInjection.Logging
{
    public class LoggerFactoryProxy : IPostSharpLoggerFactory
    {
        private readonly IMicrosoftLoggerFactory _factory;

        public LoggerFactoryProxy( IMicrosoftLoggerFactory factory )
        {
            this._factory = factory;
        }

        public IPostSharpLogger CreateLogger<T>() => new LoggerProxy( this._factory.CreateLogger<T>() );
    }
}