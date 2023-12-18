// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Diagnostics;

namespace Metalama.Backstage.Infrastructure;

internal class ProcessExecutor : IProcessExecutor
{
    public void Start( ProcessStartInfo startInfo ) => Process.Start( startInfo );
}