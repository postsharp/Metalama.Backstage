using System.Threading;

namespace Metalama.Backstage.Diagnostics;

internal sealed class LoggingContext
{
    public LoggingContext( string scope )
    {
        this.Scope = scope;
    }

    public string Scope { get; }

    public static AsyncLocal<LoggingContext> Current { get; } = new();
}