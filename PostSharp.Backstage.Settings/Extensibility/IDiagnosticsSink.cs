// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Reports warnings and errors within an application.
    /// </summary>
    public interface IDiagnosticsSink
    {
        /// <summary>
        /// Reports a warning.
        /// </summary>
        /// <param name="message">The message.</param>
        void ReportWarning( string message );

        /// <summary>
        /// Reports a warning.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Message format arguments.</param>
        void ReportWarning( string format, params object[] args );

        /// <summary>
        /// Reports an error.
        /// </summary>
        /// <param name="message">The message.</param>
        void ReportError( string message );

        /// <summary>
        /// Reports an error.
        /// </summary>
        /// <param name="format">The message format.</param>
        /// <param name="args">Message format arguments.</param>
        void ReportError( string format, params object[] args );
    }
}