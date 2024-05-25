// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Threading;

namespace Metalama.Backstage.Diagnostics;

internal sealed class LoggingContext
{
    public LoggingContext( string scope )
    {
        this.Scope = scope;
    }

    public string Scope { get; }

    public static AsyncLocal<LoggingContext?> Current { get; } = new();
}