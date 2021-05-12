// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Extensibility
{
    /// <summary>
    /// Writes trace messages.
    /// </summary>
    public interface ITrace
    {
        /// <summary>
        /// Writes a trace message followed by a new line.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteLine( string message );

        /// <summary>
        /// Writes a trace message followed by a new line.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLine( string format, params object[] args );
    }
}
