// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Diagnostics;
using System.Linq;

namespace Metalama.Backstage.Utilities;

internal class LinuxProcessManager : ProcessManagerBase
{
    private readonly ILogger _logger;

    public LinuxProcessManager( IServiceProvider serviceProvider )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "LinuxProcessManager" );
    }

    protected sealed override bool KillVbcsCompilerProcess()
    {
        var processesToKill = this.GetDotnetProcesses();

        var success = true;

        // Find dotnet processes using VBCSCompiler module.
        foreach ( var process in processesToKill )
        {
#pragma warning disable CA1307
            if ( process.Modules.OfType<ProcessModule>()
                .Any( m => m.ModuleName!.Contains( "VBCSCompiler" ) ) )
            {
                var module = process.Modules.OfType<ProcessModule>()
                    .SingleOrDefault( m => m.ModuleName!.Contains( "VBCSCompiler" ) );
#pragma warning restore CA1307
                this._logger.Trace?.Log( $"Found module {module!.ModuleName} running in {process.ProcessName} (PID: {process.Id}). Attempting to shut it down." );

                // Start process that should shutdown VBCSCompiler.
                var shutdownModuleProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "dotnet",
                        Arguments = $"{module!.FileName} -shutdown",
                        RedirectStandardOutput = true
                    }
                };

                shutdownModuleProcess.Start();
                shutdownModuleProcess.WaitForExit();

                // Try to kill the parent process (dotnet) of VBCSCompiler.dll, if shutdown didn't work.
                if ( !process.HasExited )
                {
                    this._logger.Warning?.Log( $"Couldn't shut down module '{module.ModuleName}' running in '{process.ProcessName}' (PID: {process.Id}). Attempting to kill the process." );

                    try
                    {
                        this._logger.Trace?.Log( $"Killing '{process.ProcessName}' (PID: {process.Id})." );

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
                    this._logger.Error?.Log( $"Parent process '{process.ProcessName}' (PID: {process.Id}) of module '{module.FileName}' could not be terminated." );

                    success = false;
                }

                break;
            }
        }

        return success;
    }
}