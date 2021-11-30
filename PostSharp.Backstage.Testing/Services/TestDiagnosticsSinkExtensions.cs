// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PostSharp.Backstage.Testing.Services
{
    public static class TestDiagnosticsSinkExtensions
    {
        public static TestDiagnosticsSink AsTestDiagnosticsSink( this IDiagnosticsSink diagnostics )
        {
            return (TestDiagnosticsSink)diagnostics;
        }

        public static IReadOnlyList<(string Message, IDiagnosticsLocation? Location)> GetWarnings( this IDiagnosticsSink diagnostics )
        {
            return diagnostics.AsTestDiagnosticsSink().Warnings;
        }

        public static IReadOnlyList<(string Message, IDiagnosticsLocation? Location)> GetErrors( this IDiagnosticsSink diagnostics )
        {
            return diagnostics.AsTestDiagnosticsSink().Errors;
        }

        public static void AssertNoErrors( this IDiagnosticsSink diagnostics )
        {
            diagnostics.AsTestDiagnosticsSink().AssertNoErrors();
        }

        public static void AssertNoWarnings( this IDiagnosticsSink diagnostics )
        {
            diagnostics.AsTestDiagnosticsSink().AssertNoWarnings();
        }

        public static void AssertClean( this IDiagnosticsSink diagnostics )
        {
            diagnostics.AsTestDiagnosticsSink().AssertClean();
        }

        public static void AssertSingleWarning( this IDiagnosticsSink diagnostics, string expectedMessage, IDiagnosticsLocation? expectedLocation = null )
        {
            var warning = diagnostics.AsTestDiagnosticsSink().Warnings.Single();
            Assert.Equal( expectedMessage, warning.Message );
            Assert.Equal( expectedLocation, warning.Location );
        }

        public static void AssertSingleError( this IDiagnosticsSink diagnostics, string expectedMessage, IDiagnosticsLocation? expectedLocation = null )
        {
            var error = diagnostics.AsTestDiagnosticsSink().Errors.Single();
            Assert.Equal( expectedMessage, error.Message );
            Assert.Equal( expectedLocation, error.Location );
        }
    }
}