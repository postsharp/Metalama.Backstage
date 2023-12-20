// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Diagnostics;

namespace Metalama.Backstage.Infrastructure;

internal class ProcessExecutor : IProcessExecutor
{
    public IProcess Start( ProcessStartInfo startInfo )
    {
        // Reset a few environment variables set by the Visual Studio process.
        startInfo.Environment["DOTNET_MULTILEVEL_LOOKUP"] = "";
        startInfo.Environment["DOTNET_ROOT"] = "";
        startInfo.Environment["DOTNET_STARTUP_HOOKS"] = "";
        startInfo.Environment["DOTNET_TC_CallCountThreshold"] = "";

        return new ProcessWrapper( Process.Start( startInfo ) ?? throw new InvalidOperationException( "The process could not be started." ) );
    }

    private class ProcessWrapper : IProcess
    {
        private readonly Process _process;

        public ProcessWrapper( Process process )
        {
            this._process = process;
            process.Exited += this.OnExited;
        }

        private void OnExited( object? sender, EventArgs e )
        {
            this.Exited?.Invoke();
        }

        public event Action? Exited;

        public void Dispose()
        {
            this._process.Exited -= this.OnExited;
            this._process.Dispose();
        }
    }
}