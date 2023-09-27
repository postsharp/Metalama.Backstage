// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using System;

namespace Metalama.Backstage.Testing;

internal sealed class TestUserInteractionService : IUserInteractionService
{
    public int? GetTotalMonitorWidth() => 10_000;

    public TimeSpan? GetLastInputTime() => TimeSpan.FromSeconds( 1 );
}