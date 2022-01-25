// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using Metalama.Backstage.Configuration;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Diagnostics;

[ConfigurationFile( "diagnostics.json" )]
public class DiagnosticsConfiguration : ConfigurationFile
{
    [JsonProperty( "logging" )]
    public LoggingConfiguration Logging { get; private set; } = new();

    [JsonProperty( "debugger" )]
    public DebuggerConfiguration Debugger { get; private set; } = new();

    public DiagnosticsConfiguration()
    {
        this.Reset();
    }

    public void Reset()
    {
        this.Logging.Processes = new Dictionary<ProcessKind, bool>
        {
            [ProcessKind.Compiler] = false, [ProcessKind.Rider] = false, [ProcessKind.DevEnv] = false, [ProcessKind.RoslynCodeAnalysisService] = false
        };

        this.Logging.Categories = new Dictionary<string, bool>( StringComparer.OrdinalIgnoreCase ) { ["*"] = false, ["Licensing"] = false };

        this.Debugger.Processes = new Dictionary<ProcessKind, bool>
        {
            [ProcessKind.Compiler] = false, [ProcessKind.Rider] = false, [ProcessKind.DevEnv] = false, [ProcessKind.RoslynCodeAnalysisService] = false
        };
    }

    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (DiagnosticsConfiguration) configurationFile;
        this.Logging = source.Logging.Clone();
        this.Debugger = source.Debugger.Clone();
    }
}