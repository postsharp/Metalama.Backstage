// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Diagnostics;

namespace Metalama.Backstage.Utilities;

internal class WindowsProcessManager : ProcessManagerBase
{
    private readonly ILogger _logger;

    public WindowsProcessManager( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "WindowsProcessManager" );
    }

    protected sealed override bool KillVbcsCompilerProcess()
    {
        var processes = this.GetVbcsCompilerProcesses();

        this._logger.Trace?.Log( "Hello" );
        var success = true;

        foreach ( var process in processes )
        {
            // Start process that should shutdown VBCSCompiler.
            try
            {
                if ( process.MainModule == null )
                {
                    continue;
                }

                var shutdownProcess = new Process() { StartInfo = new ProcessStartInfo() { FileName = process.MainModule.FileName, Arguments = "-shutdown" } };

                shutdownProcess.Start();
                shutdownProcess.WaitForExit();
            }
            catch ( Exception e )
            {
                this._logger.Error?.Log(
                    $"Could not shutdown process {process.ProcessName} ({process.Id}): {e.Message}." );
            }

            // Try killing the process directly, if shutting down VBCSCompiler.exe didn't work.
            if ( !process.HasExited )
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch ( InvalidOperationException e )
                {
                    this._logger.Warning?.Log( $"Could not kill process '{process.ProcessName}' (PID: {process.Id}), because it has already exited: {e.Message}." );
                }
                catch ( Exception e )
                {
                    this._logger.Error?.Log( $"Could not kill process '{process.ProcessName}' (PID: {process.Id}): {e.Message}." );
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