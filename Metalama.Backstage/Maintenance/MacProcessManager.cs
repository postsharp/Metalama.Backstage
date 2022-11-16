// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Metalama.Backstage;

internal class MacProcessManager : ProcessManagerBase
{
    public MacProcessManager( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    private bool ReferencesCompiler( Process process )
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
                if ( outputLine.ReadLine()!.Contains( "VBCSCompiler.dll" ) )
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected override IEnumerable<KillableProcess> GetProcessesToKill()
    {
        return GetDotnetProcesses().Where( this.ReferencesCompiler ).Select( p => new KillableProcess( p, this.Logger, null ) );
    }
}