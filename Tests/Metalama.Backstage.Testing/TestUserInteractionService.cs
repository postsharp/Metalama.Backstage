// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using System;

namespace Metalama.Backstage.Testing;

public sealed class TestUserInteractionService : IUserInteractionService
{
    public int? TotalMonitorWidth { get; set; } = 10_000;

    public TimeSpan? LastInputTime { get; set; } = TimeSpan.FromSeconds( 1 );
    
    public int? GetTotalMonitorWidth() => this.TotalMonitorWidth;

    public TimeSpan? GetLastInputTime() => this.LastInputTime;
}