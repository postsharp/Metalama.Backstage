// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Diagnostics
{
    public interface ILogger
    {
        /// <summary>
        /// Printed to the log file only or to the console in verbose mode.
        /// </summary>
        ILogWriter? Trace { get; }

        /// <summary>
        /// Printed to the console.
        /// </summary>
        ILogWriter? Info { get; }

        /// <summary>
        /// Warning.
        /// </summary>
        ILogWriter? Warning { get; }

        /// <summary>
        /// Error.
        /// </summary>
        ILogWriter? Error { get; }

        ILogger WithPrefix( string prefix );
    }
}