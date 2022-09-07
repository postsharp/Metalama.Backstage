// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Diagnostics
{
    public interface ILogger
    {
        ILogWriter? Trace { get; }

        ILogWriter? Info { get; }

        ILogWriter? Warning { get; }

        ILogWriter? Error { get; }

        ILogger WithPrefix( string prefix );
    }
}