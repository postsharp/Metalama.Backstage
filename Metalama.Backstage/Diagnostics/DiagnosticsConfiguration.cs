// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
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
        var processes = Enum.GetValues( typeof(ProcessKind) ).Cast<ProcessKind>().ToImmutableDictionary( x => x, x => false );

        this.Logging = new LoggingConfiguration
        {
            Processes = processes,
            Categories = ImmutableDictionary<string, bool>.Empty.WithComparers( StringComparer.OrdinalIgnoreCase ).Add( "*", false ),
        };

        this.Debugger = new DebuggerConfiguration()
        {
            Processes = processes,
        };

        this.MiniDump = new MiniDumpConfiguration()
        {
            Processes = processes,
            Flags = new[]
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
                }.Select( x => x.ToString() )
                .ToImmutableArray(),
            ExceptionTypes = ImmutableArray.Create( "*" )
        };
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