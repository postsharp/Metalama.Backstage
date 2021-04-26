﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostSharp.Backstage.Licensing
{
    public interface ILicenseConsumer
    {
        public string TargetTypeNamespace { get; }

        public string TargetTypeName { get; }

        public IDiagnosticsSink Diagnostics { get; }

        public IDiagnosticsLocation DiagnosticsLocation { get; set; }
    }
}
