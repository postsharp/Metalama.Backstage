// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Extensibility;

/// <summary>
/// Profiling service, which allows to control the current profiling session, if active.
/// </summary>
public interface IProfilingService : IBackstageService 
{
    void CreateMemorySnapshot( string? snapshotName = null );
}