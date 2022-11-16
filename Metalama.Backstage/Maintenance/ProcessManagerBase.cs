// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Metalama.Backstage;

internal abstract partial class ProcessManagerBase : IProcessManager
{
    protected ILogger Logger { get; }

    protected ProcessManagerBase( IServiceProvider serviceProvider )
    {
        this.Logger = serviceProvider.GetLoggerFactory().GetLogger( "ProcessManager" );
    }

    protected static Process[] GetDotnetProcesses() => Process.GetProcessesByName( "dotnet" );

    protected IEnumerable<KillableProcess> GetDotNetCompilerProcesses()
        => GetDotnetProcesses()
            .Select(
                p => (Process: p, Module: p.Modules.OfType<ProcessModule>()
                          .FirstOrDefault( m => m.ModuleName!.Contains( "VBCSCompiler" ) )) )
            .Where( p => p.Module != null )
            .Select( p => new KillableProcess( p.Process, this.Logger, p.Module ) );

    protected abstract IEnumerable<KillableProcess> GetProcessesToKill();

    public virtual void KillCompilerProcesses()
    {
        foreach ( var process in this.GetProcessesToKill() )
        {
            process.ShutdownOrKill();
        }
    }
}