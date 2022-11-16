// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Diagnostics;

namespace Metalama.Backstage;

internal abstract partial class ProcessManagerBase
{
    protected class KillableProcess
    {
        private readonly ILogger _logger;

        private readonly Process _process;

        private readonly ProcessModule? _compilerModule;

        private void Shutdown()
        {
            try
            {
                this._logger.Trace?.Log( $"Gracefully shutting down process {this._process.Id}." );

                var shutdownProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = this._process.MainModule!.FileName,
                        Arguments = this._compilerModule == null ? $"\"{this._compilerModule}\" -shutdown" : "-shutdown",
                        RedirectStandardOutput = true
                    }
                };

                shutdownProcess.Start();
                shutdownProcess.WaitForExit();
            }
            catch ( Exception e )
            {
                this._logger.Warning?.Log(  $"Unable to gracefully shut down process {this._process.Id}: {e.Message}" );
            }
        }

        public void ShutdownOrKill()
        {
            this.Shutdown();
            this.Kill();
        }

        private void Kill()
        {
            var process = this._process;

            // Try killing the process directly, if shutting down VBCSCompiler.exe didn't work.
            if ( this._process.HasExited )
            {
                return;
            }

            try
            {
                this._logger.Trace?.Log( $"Killing '{this._process.ProcessName}' (PID: {process.Id})." );

                process.Kill();
                process.WaitForExit();
            }
            catch ( InvalidOperationException ) when ( process.HasExited )
            {
                // Nothing to do. We lost a race that ended the process.   
            }
            catch ( Exception e )
            {
                this._logger.Error?.Log( $"Could not kill process '{process.ProcessName}' (PID: {process.Id}): {e.Message}." );
            }
        }

        public KillableProcess( Process process, ILogger logger, ProcessModule? compilerModule )
        {
            this._process = process;
            this._logger = logger;
            this._compilerModule = compilerModule;
        }
    }
}