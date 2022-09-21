// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Backstage.Diagnostics;

[ConfigurationFile( "diagnostics.json", EnvironmentVariableName = EnvironmentVariableName )]
public record DiagnosticsConfiguration : ConfigurationFile
{
    public const string EnvironmentVariableName = "METALAMA_DIAGNOSTICS";
    
    [JsonProperty( "logging" )]
    public LoggingConfiguration Logging { get; init; } = new();

    [JsonProperty( "debugger" )]
    public DebuggerConfiguration Debugger { get; } = new();

    [JsonProperty( "miniDump" )]
    public MiniDumpConfiguration MiniDump { get; } = new();

    public DiagnosticsConfiguration()
    {
        this.Logging.Processes = new Dictionary<ProcessKind, bool>
        {
            [ProcessKind.Compiler] = false, [ProcessKind.Rider] = false, [ProcessKind.DevEnv] = false, [ProcessKind.RoslynCodeAnalysisService] = false
        };

        this.Logging.Categories = new Dictionary<string, bool>( StringComparer.OrdinalIgnoreCase ) { ["*"] = true };

        this.Debugger.Processes = new Dictionary<ProcessKind, bool>
        {
            [ProcessKind.Compiler] = false, [ProcessKind.Rider] = false, [ProcessKind.DevEnv] = false, [ProcessKind.RoslynCodeAnalysisService] = false
        };

        this.MiniDump.Processes = new Dictionary<ProcessKind, bool>
        {
            [ProcessKind.Compiler] = false, [ProcessKind.Rider] = false, [ProcessKind.DevEnv] = false, [ProcessKind.RoslynCodeAnalysisService] = false
        };

        this.MiniDump.Flags.Clear();

        this.MiniDump.Flags.AddRange(
            new[]
            {
                MiniDumpKind.WithDataSegments,
                MiniDumpKind.WithProcessThreadData,
                MiniDumpKind.WithHandleData,
                MiniDumpKind.WithPrivateReadWriteMemory,
                MiniDumpKind.WithUnloadedModules,
                MiniDumpKind.WithFullMemoryInfo,
                MiniDumpKind.WithThreadInfo,
                MiniDumpKind.FilterMemory,
                MiniDumpKind.WithoutAuxiliaryState
            }.Select( x => x.ToString() ) );

        this.MiniDump.ExceptionTypes = new List<string> { "*" };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticsConfiguration"/> class from existing properties values.
    /// </summary>
    /// <param name="logging"></param>
    /// <param name="debugger"></param>
    /// <param name="miniDump"></param>
    [JsonConstructor]
    public DiagnosticsConfiguration( LoggingConfiguration logging, DebuggerConfiguration debugger, MiniDumpConfiguration miniDump )
    {
        this.Logging = logging;
        this.Debugger = debugger;
        this.MiniDump = miniDump;
    }
}