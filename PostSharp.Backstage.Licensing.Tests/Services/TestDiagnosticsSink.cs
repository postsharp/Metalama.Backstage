// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal class TestDiagnosticsSink : IDiagnosticsSink
    {
        private readonly List<string> _warnings = new();
        private readonly List<string> _errors = new();

        public IReadOnlyList<string> Warnings => this._warnings;

        public IReadOnlyList<string> Errors => this._errors;

        public void ReportWarning( string message )
        {
            this._warnings.Add( message );
        }

        public void ReportError( string message )
        {
            this._errors.Add( message );
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
            if ( this._warnings.Count != 0 )
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
