// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics;

namespace Metalama.Backstage.Testing;

public class TestProcessExecutor : IProcessExecutor
{
    public List<ProcessStartInfo> StartedProcesses { get; } = new();

    public Process? Start( ProcessStartInfo startInfo )
    {
        this.StartedProcesses.Add( startInfo );

        return null;
    }
}