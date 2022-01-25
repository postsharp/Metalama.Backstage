// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Diagnostics
{
    public static class TracingExtensions
    {
        public static ILoggerFactory GetLoggerFactory( this IServiceProvider services ) => services.GetService<ILoggerFactory>() ?? NullLogger.Instance;
    }
}