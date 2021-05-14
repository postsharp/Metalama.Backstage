// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.IO;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Cli.Console
{
    internal class ConsoleTrace : ITrace
    {
        private readonly IConsole _console;

        public ConsoleTrace( IConsole console )
        {
            this._console = console;
        }

        public void WriteLine( string message )
        {
            this._console.Out.WriteLine( message );
        }
    }
}
