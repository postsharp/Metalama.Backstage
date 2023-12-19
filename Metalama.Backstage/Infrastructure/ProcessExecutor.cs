// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Diagnostics;

namespace Metalama.Backstage.Infrastructure;

internal class ProcessExecutor : IProcessExecutor
{
    public IProcess Start( ProcessStartInfo startInfo )
        => new ProcessWrapper( Process.Start( startInfo ) ?? throw new InvalidOperationException( "The process could not be started." ) );

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