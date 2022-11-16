// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Metalama.Backstage;

internal class MacProcessManager : ProcessManagerBase
{
    public MacProcessManager( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    private static bool ReferencesModule( ImmutableArray<KillableModuleSpec> processNames, Process process )
    {
        var listOpenFilesProcess = new Process()
        {
            StartInfo = new ProcessStartInfo() { FileName = "lsof", Arguments = $"-p {process.Id}", RedirectStandardOutput = true }
        };

        listOpenFilesProcess.Start();

        while ( !listOpenFilesProcess.StandardOutput.EndOfStream )
        {
            var outputLine = listOpenFilesProcess.StandardOutput;

            if ( outputLine.ReadLine() != null )
            {
                // TODO: pass the complete path for the shutdown logic.
                if ( processNames.Any( n => n.IsDotNet && outputLine.ReadLine()!.Contains( n.Name ) ) )
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected override IEnumerable<KillableProcess> GetProcesses( ImmutableArray<KillableModuleSpec> processNames )
    {
        return GetDotnetProcesses().Where( p => ReferencesModule( processNames, p ) ).Select( p => new KillableProcess( p, this.Logger, null ) );
    }
}