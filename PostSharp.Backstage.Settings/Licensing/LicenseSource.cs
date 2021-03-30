// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing
{
    [Flags]
#pragma warning disable CA1714 // Flags enums should have plural names (TODO)
    public enum LicenseSource : long
#pragma warning restore CA1714 // Flags enums should have plural names
    {
        All = -1,
        None = 0,
        CurrentUserRegistry = 1,
        AllUsersRegistry = 2,
        Internal = 4,
        Programmatic = 8,
        Configuration = 16,
        CommandLine = 32
    }
}
