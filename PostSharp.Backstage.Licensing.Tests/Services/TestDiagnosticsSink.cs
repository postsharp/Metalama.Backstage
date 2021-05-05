// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal class TestDiagnosticsSink : IDiagnosticsSink
    {
        private readonly TestTrace _trace;
        private readonly List<string> _warnings = new();
        private readonly List<string> _errors = new();

        public string Name { get; set; }

        public IReadOnlyList<string> Warnings => this._warnings;

        public IReadOnlyList<string> Errors => this._errors;

        public TestDiagnosticsSink( TestTrace trace, [CallerMemberName] string name = "" )
        {
            this.Name = name;
            this._trace = trace;
        }

        private void Trace( string verbosity, string message )
        {
            this._trace.WriteLine( "Diagnostic sink '{0}' reported '{1}': {2}", this.Name, verbosity, message );
        }

        public void ReportWarning( string message )
        {
            this.Trace( "warning", message );
            this._warnings.Add( message );
        }

        public void ReportWarning( string format, params object[] args )
        {            
            this.ReportWarning( string.Format( CultureInfo.InvariantCulture, format, args ) );
        }

        public void ReportError( string message )
        {
            this.Trace( "error", message );
            this._errors.Add( message );
        }

        public void ReportError( string format, params object[] args )
        {
            this.ReportError( string.Format( CultureInfo.InvariantCulture, format, args ) );
        }

        public void AssertNoWarnings()
        {
            if ( this._warnings.Count != 0 )
            {
                throw new InvalidOperationException( string.Join( Environment.NewLine, this._warnings.Prepend( "Warnings:" ) ) );
            }
        }

        public void AssertNoErrors()
        {
            if ( this._errors.Count != 0 )
            {
                throw new InvalidOperationException( string.Join( Environment.NewLine, this._errors.Prepend( "Errors:" ) ) );
            }
        }

        public void AssertClean()
        {
            if ( this._warnings.Count != 0 && this._errors.Count != 0 )
            {
                throw new InvalidOperationException( string.Join( Environment.NewLine, this._warnings.Prepend( "Warnings:" ).Union( this._errors.Prepend( "Errors:" ) ) ) );
            }
            else
            {
                this.AssertNoWarnings();
                this.AssertNoErrors();
            }
        }
    }
}
