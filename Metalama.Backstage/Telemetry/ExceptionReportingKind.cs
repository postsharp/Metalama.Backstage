// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Backstage.Telemetry
{
    /// <summary>
    /// Represents what kind of issue is being reported.
    /// </summary>
    public enum ExceptionReportingKind
    {
        /// <summary>
        /// An actual exception, other than VS extension's MainThreadBlockedException, is being reported.
        /// </summary>
        Exception,

        /// <summary>
        /// The main thread of Visual Studio is blocked for a long time.
        /// </summary>
        PerformanceProblem
    }
}