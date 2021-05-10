// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.IO;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Cli.Console
{
    internal class ConsoleDiagnosticsSink : IDiagnosticsSink
    {
        private readonly IConsole _console;

        public ConsoleDiagnosticsSink( IConsole console )
        {
            this._console = console;
        }

        public void ReportError( string message )
        {
            this._console.Error.WriteLine( message );
        }

        public void ReportError( string format, params object[] args )
        {
            this.ReportError( string.Format( format, args ) );
        }

        public void ReportWarning( string message )
        {
            this._console.Out.WriteLine( message );
        }

        public void ReportWarning( string format, params object[] args )
        {
            this.ReportWarning( string.Format( format, args ) );
        }
    }
}
