// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PostSharp.Backstage.Testing.Services
{
    public class TestDiagnosticsSink : IBackstageDiagnosticSink
    {
        private readonly ILogger _logger;
        private readonly List<(string, IDiagnosticLocation?)> _warnings = new();
        private readonly List<(string, IDiagnosticLocation?)> _errors = new();

        public string Name { get; set; }

        public IReadOnlyList<(string Message, IDiagnosticLocation? Location)> Warnings => this._warnings;

        public IReadOnlyList<(string Message, IDiagnosticLocation? Location)> Errors => this._errors;

        public TestDiagnosticsSink( IServiceProvider services, [CallerMemberName] string name = "" )
        {
            this.Name = name;
            this._logger = services.GetLogger<TestLogCategory>()!;
        }

        private void Trace( string verbosity, string message, IDiagnosticLocation? location )
        {
            this._logger.Trace?.Log( $"Diagnostic sink '{this.Name}' reported '{verbosity}' at '{location?.ToString() ?? "unknown"}': {message}" );
        }

        public void ReportWarning( string message, IDiagnosticLocation? location = null )
        {
            this.Trace( "warning", message, location );
            this._warnings.Add( (message, location) );
        }

        public void ReportError( string message, IDiagnosticLocation? location = null )
        {
            this.Trace( "error", message, location );
            this._errors.Add( (message, location) );
        }

        private static IEnumerable<string> DiagnosticsToString( IEnumerable<(string Message, IDiagnosticLocation? Location)> diagnostics )
        {
            return diagnostics.Select( d => $"{d.Message} at {d.Location?.ToString() ?? "unknown"}" );
        }

        public void AssertNoWarnings()
        {
            if ( this._warnings.Count != 0 )
            {
                throw new InvalidOperationException(
                    string.Join(
                        Environment.NewLine,
                        DiagnosticsToString( this._warnings ).Prepend( "Warnings:" ) ) );
            }
        }

        public void AssertNoErrors()
        {
            if ( this._errors.Count != 0 )
            {
                throw new InvalidOperationException(
                    string.Join(
                        Environment.NewLine,
                        DiagnosticsToString( this._errors ).Prepend( "Errors:" ) ) );
            }
        }

        public void AssertClean()
        {
            if ( this._warnings.Count != 0 && this._errors.Count != 0 )
            {
                throw new InvalidOperationException(
                    string.Join(
                        Environment.NewLine,
                        DiagnosticsToString( this._warnings )
                            .Prepend( "Warnings:" )
                            .Union( DiagnosticsToString( this._errors ).Prepend( "Errors:" ) ) ) );
            }
            else
            {
                this.AssertNoWarnings();
                this.AssertNoErrors();
            }
        }
    }
}