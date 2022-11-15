// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Diagnostics;

namespace Metalama.Backstage.Utilities;

internal class MacProcessManager : ProcessManagerBase
{
    private readonly ILogger _logger;

    public MacProcessManager( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "MacOSXProcessManager" );
    }

    protected sealed override bool KillVbcsCompilerProcess()
    {
        var processesToKill = this.GetDotnetProcesses();

        var success = true;

        // For each dotnet process get list of open files the process uses, then kill the process.
        foreach ( var process in processesToKill )
        {
            var listOpenFilesProcess = new Process() { StartInfo = new ProcessStartInfo() { FileName = "lsof", Arguments = $"-p {process.Id}", RedirectStandardOutput = true } };
            listOpenFilesProcess.Start();

            var processUsesVbcsCompiler = false;

            while ( !listOpenFilesProcess.StandardOutput.EndOfStream )
            {
                var outputLine = listOpenFilesProcess.StandardOutput;

                if ( outputLine.ReadLine() != null )
                {
#pragma warning disable CA1307
                    processUsesVbcsCompiler = outputLine.ReadLine()!.Contains( "VBCSCompiler.dll" );
#pragma warning restore CA1307
                }

                if ( processUsesVbcsCompiler )
                {
                    break;
                }
            }

            listOpenFilesProcess.WaitForExit();

            if ( listOpenFilesProcess.ExitCode != 0 )
            {
                this._logger.Error?.Log( $"Could not list open files for process '{process.ProcessName}' (PID:{process.Id})." );

                success = false;
            }

            if ( processUsesVbcsCompiler )
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch ( InvalidOperationException e )
                {
                    this._logger.Warning?.Log( $"Could not kill process '{process.ProcessName}' (PID: {process.Id}), because it has already exited: {e.Message}" );
                }
                catch ( Exception e )
                {
                    this._logger.Error?.Log( $"{e.Message}" );
                }
            }

            if ( !process.HasExited )
            {
                this._logger.Error?.Log( $"Process '{process.ProcessName}' (PID: {process.Id}) could not be terminated." );

                success = false;
            }
        }

        return success;
    }
}