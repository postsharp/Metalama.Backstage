// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.Maintenance;

public interface IProcessManager : IBackstageService
{
    public void KillCompilerProcesses( bool shouldEmitWarnings );
}