// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Licensing.Consumption.Sources;

[Flags]
public enum LicenseSourceKind
{
    None,
    Unattended = 1,
    UserProfile = 2,
    Explicit = 4,
    All = Unattended | UserProfile | Explicit
}