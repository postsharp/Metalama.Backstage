// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.IO;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Cli.Console
{
    public class ConsoleTrace : ITrace
    {
        private readonly IConsole _console;

        public bool IsEnabled { get; set; }

        public ConsoleTrace( IConsole console )
        {
            this._console = console;
        }

        public void WriteLine( string message )
        {
            if ( !this.IsEnabled )
            {
                return;
            }

            this._console.Out.WriteLine( message );
        }

        public void WriteLine( string format, params object[] args )
        {
            this.WriteLine( string.Format( format, args ) );
        }
    }
}
