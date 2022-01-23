// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Diagnostics
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(string category);
    }

    internal static class LoggerFactoryExtensions
    {
        public static ILogger Licensing( this ILoggerFactory loggerFactory ) => loggerFactory.GetLogger( "Licensing" );
        public static ILogger Configuration( this ILoggerFactory loggerFactory ) => loggerFactory.GetLogger( "Configuration" );
        public static ILogger Test( this ILoggerFactory loggerFactory ) => loggerFactory.GetLogger( "Test" );
    }

    public interface IDebuggerService
    {
        void LaunchDebugger();
    }

}