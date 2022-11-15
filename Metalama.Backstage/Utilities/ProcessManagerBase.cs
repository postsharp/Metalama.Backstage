// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Diagnostics;

namespace Metalama.Backstage.Utilities;

public abstract class ProcessManagerBase : IProcessManager
{
    public Process[] GetDotnetProcesses() => Process.GetProcessesByName( "dotnet" );

    public Process[] GetVbcsCompilerProcesses() => Process.GetProcessesByName( "VBCSCompiler" );

    public bool RunKillVbcsCompiler() => this.KillVbcsCompilerProcess();

    protected abstract bool KillVbcsCompilerProcess();
}