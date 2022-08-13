// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Backstage.Diagnostics;

[ConfigurationFile( "diagnostics.json" )]
public class DiagnosticsConfiguration : ConfigurationFile
{
    [JsonProperty( "logging" )]
    public LoggingConfiguration Logging { get; private set; } = new();

    [JsonProperty( "debugger" )]
    public DebuggerConfiguration Debugger { get; private set; } = new();

    [JsonProperty( "miniDump" )]
    public MiniDumpConfiguration MiniDump { get; private set; } = new();

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

    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (DiagnosticsConfiguration) configurationFile;
        this.Logging = source.Logging.Clone();
        this.Debugger = source.Debugger.Clone();
    }
}