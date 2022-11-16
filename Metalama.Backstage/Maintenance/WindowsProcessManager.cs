// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Metalama.Backstage;

internal class WindowsProcessManager : ProcessManagerBase
{
    public WindowsProcessManager( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    protected override IEnumerable<KillableProcess> GetProcesses( ImmutableArray<KillableModuleSpec> processNames )
        => this.GetDotNetCompilerProcesses( processNames ).Concat( this.GetStandaloneCompilerProcesses( processNames ) );

    private IEnumerable<KillableProcess> GetStandaloneCompilerProcesses( ImmutableArray<KillableModuleSpec> processNames )
        => processNames.Where( n => n.IsStandaloneProcess )
            .SelectMany( n => Process.GetProcessesByName( n.Name ).Select( p => new KillableProcess( p, this.Logger, null ) ) );
}