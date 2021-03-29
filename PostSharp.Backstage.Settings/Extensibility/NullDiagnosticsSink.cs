// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Extensibility
{
    public sealed class NullDiagnosticsSink : IDiagnosticsSink
    {
        public static readonly NullDiagnosticsSink Instance = new NullDiagnosticsSink();

        private NullDiagnosticsSink()
        {
        }

        public void ReportError( string message )
        {
        }

        public void ReportWarning( string message )
        {
        }
    }
}
