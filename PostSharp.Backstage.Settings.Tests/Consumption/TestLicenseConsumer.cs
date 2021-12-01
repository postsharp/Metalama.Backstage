// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Testing.Services;
using System;

namespace PostSharp.Backstage.Licensing.Tests.Consumption
{
    internal class TestLicenseConsumer : ILicenseConsumer
    {
        public string TargetTypeNamespace { get; }

        public string TargetTypeName { get; }

        public IBackstageDiagnosticSink Diagnostics { get; }

        public IDiagnosticsLocation DiagnosticsLocation { get; }

        public TestLicenseConsumer( string targetTypeNamespace, string targetTypeName, string diagnosticsLocationDescription, IServiceProvider services )
        {
            this.TargetTypeNamespace = targetTypeNamespace;
            this.TargetTypeName = targetTypeName;
            this.Diagnostics = new TestDiagnosticsSink( services );
            this.DiagnosticsLocation = new TestDiagnosticsLocation( diagnosticsLocationDescription );
        }
    }
}