// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility.Extensions;
using PostSharp.Backstage.Logging;
using System;

namespace PostSharp.Backstage.Extensibility
{
    public static class TracingExtensions
    {
        public static ILogger GetLogger<T>( this IServiceProvider services )
            where T : ILogCategory, new()
            => services.GetService<ILoggerFactory>()?.CreateLogger<T>() ?? NullLogger.Instance;
    }
}