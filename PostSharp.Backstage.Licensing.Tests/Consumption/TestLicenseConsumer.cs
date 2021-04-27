// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Consumption;
using PostSharp.Backstage.Licensing.Tests.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostSharp.Backstage.Licensing.Tests.Consumption
{
    public class TestLicenseConsumer : ILicenseConsumer
    {
        public string TargetTypeNamespace { get; }

        public string TargetTypeName { get; }

        public IDiagnosticsSink Diagnostics { get; } = new TestDiagnosticsSink();

        public IDiagnosticsLocation DiagnosticsLocation { get; }

        public TestLicenseConsumer( string targetTypeNamespace, string targetTypeName, IDiagnosticsLocation diagnosticsLocation )
        {
            this.TargetTypeNamespace = targetTypeNamespace;
            this.TargetTypeName = targetTypeName;
            this.DiagnosticsLocation = diagnosticsLocation;
        }
    }
}
