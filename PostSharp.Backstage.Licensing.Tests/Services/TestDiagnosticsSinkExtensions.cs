// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Licensing.Tests.Services
{
    internal static class TestDiagnosticsSinkExtensions
    {
        public static TestDiagnosticsSink ToTestDiagnosticsSink( this IDiagnosticsSink diagnostics )
        {
            return (TestDiagnosticsSink) diagnostics;
        }

        public static IReadOnlyList<string> GetWarnings( this IDiagnosticsSink diagnostics )
        {
            return diagnostics.ToTestDiagnosticsSink().Warnings;
        }

        public static IReadOnlyList<string> GetErrors( this IDiagnosticsSink diagnostics )
        {
            return diagnostics.ToTestDiagnosticsSink().Errors;
        }

        public static void AssertNoErrors(this IDiagnosticsSink diagnostics)
        {
            diagnostics.ToTestDiagnosticsSink().AssertNoErrors();
        }

        public static void AssertNoWarnings( this IDiagnosticsSink diagnostics )
        {
            diagnostics.ToTestDiagnosticsSink().AssertNoWarnings();
        }

        public static void AssertClean( this IDiagnosticsSink diagnostics )
        {
            diagnostics.ToTestDiagnosticsSink().AssertClean();
        }
    }
}
