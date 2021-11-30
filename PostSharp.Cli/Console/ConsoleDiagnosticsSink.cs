// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using System;
using System.CommandLine;
using System.CommandLine.IO;

namespace PostSharp.Cli.Console
{
    internal class ConsoleDiagnosticsSink : IDiagnosticsSink
    {
        private readonly IConsole _console;

        public ConsoleDiagnosticsSink( IServiceProvider services )
        {
            _console = services.GetRequiredService<IConsole>();
        }

        public void ReportError( string message, IDiagnosticsLocation? location = null )
        {
            _console.Error.WriteLine( message );
        }

        public void ReportWarning( string message, IDiagnosticsLocation? location = null )
        {
            ReportError( message );
        }
    }
}