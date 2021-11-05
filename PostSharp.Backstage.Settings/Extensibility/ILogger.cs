// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Diagnostics;

namespace PostSharp.Backstage.Extensibility
{
    public interface ILogger
    {
        bool IsEnabled( LogLevel logLevel );

        void LogTrace( string message );

        void LogInformation( string message );
    }
}