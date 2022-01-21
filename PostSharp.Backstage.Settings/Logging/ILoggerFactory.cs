// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger<T>()
            where T : ILogCategory, new();
    }

    public interface ILogCategory
    {
        string Name { get; }
    }
}