// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    ///   Receives messages.
    /// </summary>
    /// <remarks>
    ///   Use this interface instead of events for cross-domain communication.
    /// </remarks>
    public interface IDiagnosticsSink : IService
    {
        /// <summary>
        /// Reports a warning.
        /// </summary>
        /// <param name = "message">A message.</param>
        void ReportWarning( string message );

        /// <summary>
        /// Reports an error.
        /// </summary>
        /// <param name = "message">A message.</param>
        void ReportError( string message );
    }
}