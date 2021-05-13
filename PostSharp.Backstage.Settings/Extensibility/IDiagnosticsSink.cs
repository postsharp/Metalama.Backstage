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
        /// <param name="location">The location related to the message.</param>
        void ReportWarning( string message, IDiagnosticsLocation? location = null );

        /// <summary>
        /// Reports an error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="location">The location related to the message.</param>
        void ReportError( string message, IDiagnosticsLocation? location = null );
    }
}