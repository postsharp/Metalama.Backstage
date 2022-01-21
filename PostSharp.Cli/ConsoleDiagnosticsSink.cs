// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using System;
using System.CommandLine;
using System.CommandLine.IO;

namespace PostSharp.Cli
{
    internal class ConsoleDiagnosticsSink : IBackstageDiagnosticSink
    {
        private readonly IConsole _console;

        public ConsoleDiagnosticsSink( IServiceProvider services )
        {
            this._console = services.GetRequiredService<IConsole>();
        }

        public void ReportError( string message, IDiagnosticLocation? location = null )
        {
            this._console.Error.WriteLine( message );
        }

        public void ReportWarning( string message, IDiagnosticLocation? location = null )
        {
            this.ReportError( message );
        }
    }
}