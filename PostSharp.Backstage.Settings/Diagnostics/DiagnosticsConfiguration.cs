// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using PostSharp.Backstage.Configuration;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Diagnostics;

public class DiagnosticsConfiguration : ConfigurationFile
{
    [JsonProperty("logging")]
    public LoggingConfiguration Logging { get; } = new LoggingConfiguration();

    [JsonProperty("debugger")]
    public DebuggerConfiguration Debugger { get; } = new DebuggerConfiguration();

    public DiagnosticsConfiguration()
    {
        this.Reset();
    }

    public void Reset()
    {
        this.Logging.Processes = new Dictionary<ProcessKind, bool>
        {
            [ProcessKind.Compiler] = false,
            [ProcessKind.Rider] = false,
            [ProcessKind.DevEnv] = false,
            [ProcessKind.RoslynCodeAnalysisService] = false
        };
        
        this.Logging.Categories = new Dictionary<string, bool>( StringComparer.OrdinalIgnoreCase ) { ["*"] = false, ["Licensing"] = false };
        
        this.Debugger.Processes = new Dictionary<ProcessKind, bool>
        {
            [ProcessKind.Compiler] = false,
            [ProcessKind.Rider] = false,
            [ProcessKind.DevEnv] = false,
            [ProcessKind.RoslynCodeAnalysisService] = false
        };
    }

    public static DiagnosticsConfiguration Load( IServiceProvider services ) => Load<DiagnosticsConfiguration>( services );

    public override string FileName => "logging.json";
}

