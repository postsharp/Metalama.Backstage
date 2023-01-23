// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Backstage.Maintenance;

public enum CleanUpStrategy
{
    // The integer values must not be changed because they are serialized in cleanup.json.

    // Resharper disable once UnusedMember.Global.
    None = 0,
    Always = 1,
    WhenUnused = 2
}