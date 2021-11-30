// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PostSharp.Backstage.Testing.Services
{
    public class TestDiagnosticsSink : IDiagnosticsSink
    {
        private readonly ILogger _logger;
        private readonly List<(string, IDiagnosticsLocation?)> _warnings = new();
        private readonly List<(string, IDiagnosticsLocation?)> _errors = new();

        public string Name { get; set; }

        public IReadOnlyList<(string Message, IDiagnosticsLocation? Location)> Warnings => _warnings;

        public IReadOnlyList<(string Message, IDiagnosticsLocation? Location)> Errors => _errors;

        public TestDiagnosticsSink( IServiceProvider services, [CallerMemberName] string name = "" )
        {
            Name = name;
            _logger = services.GetOptionalTraceLogger<TestDiagnosticsSink>()!;
        }

        private void Trace( string verbosity, string message, IDiagnosticsLocation? location )
        {
            _logger.LogTrace( $"Diagnostic sink '{Name}' reported '{verbosity}' at '{location?.ToString() ?? "unknown"}': {message}" );
        }

        public void ReportWarning( string message, IDiagnosticsLocation? location = null )
        {
            Trace( "warning", message, location );
            _warnings.Add( ( message, location ) );
        }

        public void ReportError( string message, IDiagnosticsLocation? location = null )
        {
            Trace( "error", message, location );
            _errors.Add( ( message, location ) );
        }

        private static IEnumerable<string> DiagnosticsToString( IEnumerable<(string Message, IDiagnosticsLocation? Location)> diagnostics )
        {
            return diagnostics.Select( d => $"{d.Message} at {d.Location?.ToString() ?? "unknown"}" );
        }

        public void AssertNoWarnings()
        {
            if (_warnings.Count != 0)
            {
                throw new InvalidOperationException( string.Join( Environment.NewLine, DiagnosticsToString( _warnings ).Prepend( "Warnings:" ) ) );
            }
        }

        public void AssertNoErrors()
        {
            if (_errors.Count != 0)
            {
                throw new InvalidOperationException( string.Join( Environment.NewLine, DiagnosticsToString( _errors ).Prepend( "Errors:" ) ) );
            }
        }

        public void AssertClean()
        {
            if (_warnings.Count != 0 && _errors.Count != 0)
            {
                throw new InvalidOperationException(
                    string.Join(
                        Environment.NewLine,
                        DiagnosticsToString( _warnings ).Prepend( "Warnings:" ).Union( DiagnosticsToString( _errors ).Prepend( "Errors:" ) ) ) );
            }
            else
            {
                AssertNoWarnings();
                AssertNoErrors();
            }
        }
    }
}