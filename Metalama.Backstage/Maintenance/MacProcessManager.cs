// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Metalama.Backstage;

internal class MacProcessManager : ProcessManagerBase
{
    public MacProcessManager( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    private static string? GetModule( ImmutableArray<KillableModuleSpec> moduleNames, Process process )
    {
        var listOpenFilesProcess = new Process()
        {
            StartInfo = new ProcessStartInfo() { FileName = "lsof", Arguments = $"-p {process.Id}", RedirectStandardOutput = true }
        };

        listOpenFilesProcess.Start();
        listOpenFilesProcess.WaitForExit();

#pragma warning disable CA1307
        while ( !listOpenFilesProcess.StandardOutput.EndOfStream )
        {
            var outputLine = listOpenFilesProcess.StandardOutput.ReadLine();

            if ( outputLine != null )
            {
                if ( moduleNames.Any( n => n.IsDotNet && Path.GetFileNameWithoutExtension( outputLine ).Equals( n.Name, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    // The last substring is an actual file path.
                    return outputLine.Split( ' ' ).LastOrDefault();
                }
            }
        }

        return null;
    }
#pragma warning restore CA1307

    protected override IEnumerable<KillableProcess> GetProcesses( ImmutableArray<KillableModuleSpec> processNames )
    {
        return GetDotnetProcesses()
            .Select( p => (Process: p, Module: GetModule( processNames, p )) )
            .Where( p => p.Module != null )
            .Select( p => new KillableProcess( p.Process, this.Logger, p.Module ) );
    }
}