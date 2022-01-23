// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using ILoggerFactory = PostSharp.Backstage.Diagnostics.ILoggerFactory;
using IMicrosoftLoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using IPostSharpLogger = PostSharp.Backstage.Diagnostics.ILogger;
using IPostSharpLoggerFactory = PostSharp.Backstage.Diagnostics.ILoggerFactory;

namespace PostSharp.Backstage.MicrosoftLogging
{
    internal class LoggerFactoryAdapter : ILoggerFactory
    {
        private readonly IMicrosoftLoggerFactory _factory;

        public LoggerFactoryAdapter( IMicrosoftLoggerFactory factory )
        {
            this._factory = factory;
        }

        public IPostSharpLogger GetLogger(string category)
            => new LoggerAdapter( this._factory.CreateLogger(category) );
    }
}