// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Maintenance;

internal class LinuxProcessManager : ProcessManagerBase
{
    public LinuxProcessManager( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    protected override IEnumerable<KillableProcess> GetProcessesToKill() => this.GetDotNetCompilerProcesses();
}