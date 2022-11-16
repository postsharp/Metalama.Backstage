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

        public Process Process { get; }

        private readonly string? _compilerModule;
        
        private void Shutdown()
        {
            try
            {
                this._logger.Trace?.Log( $"Gracefully shutting down process {this.Process.Id}." );

                var shutdownProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = this.Process.MainModule!.FileName,
                        Arguments = this._compilerModule != null ? $"\"{this._compilerModule}\" -shutdown" : "-shutdown",
                        RedirectStandardOutput = true
                    }
                };

                shutdownProcess.Start();
                shutdownProcess.WaitForExit();
            }
            catch ( Exception e )
            {
                this._logger.Warning?.Log( $"Unable to gracefully shut down process {this.Process.Id}: {e.Message}" );
            }
        }

        public void ShutdownOrKill()
        {
            this.Shutdown();
            this.Kill();
        }

        private void Kill()
        {
            var process = this.Process;

            // Try killing the process directly, if shutting down VBCSCompiler.exe didn't work.
            if ( process.HasExited )
            {
                return;
            }

            try
            {
                this._logger.Trace?.Log( $"Killing '{process.ProcessName}' (PID: {process.Id})." );

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

        public KillableProcess( Process process, ILogger logger, string? compilerModule )
        {
            this.Process = process;
            this._logger = logger;
            this._compilerModule = compilerModule;
        }
    }
}