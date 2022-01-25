// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Backstage.Diagnostics
{
    public interface ILogger
    {
        ILogWriter? Trace { get; }

        ILogWriter? Info { get; }

        ILogWriter? Warning { get; }

        ILogWriter? Error { get; }
    }
}