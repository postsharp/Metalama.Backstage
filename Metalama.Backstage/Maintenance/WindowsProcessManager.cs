// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Metalama.Backstage;

internal class WindowsProcessManager : ProcessManagerBase
{
    public WindowsProcessManager( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    protected override IEnumerable<KillableProcess> GetProcessesToKill() => this.GetDotNetCompilerProcesses().Concat( this.GetNetFrameworkCompilerProcesses() );

    private IEnumerable<KillableProcess> GetNetFrameworkCompilerProcesses()
        => Process.GetProcessesByName( "VBCSCompiler" ).Select( p => new KillableProcess( p, this.Logger, null ) );
}