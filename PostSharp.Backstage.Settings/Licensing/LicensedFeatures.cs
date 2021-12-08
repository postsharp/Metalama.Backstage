// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing
{
    /// <summary>
    /// Features of PostSharp for the purpose of license checking.
    /// The names and granularity correspond to the NuGet packages.
    /// </summary>
    [Flags]

    // ReSharper disable once EnumUnderlyingTypeIsInt
    public enum LicensedFeatures : int
    {
        None = 0,

        // Some features are free in the Community Edition (e.g. non-semantic OnMethodBoundaryAspect), but those transformations
        // also need at least some license to be loaded. Licenses can only be loaded if a requirement is present.
        Community = 1,
        Common = 1 << 1,
        Framework = 1 << 2,
        Threading = 1 << 3,
        Model = 1 << 4,
        Xaml = 1 << 5,
        Aggregation = 1 << 6,
        Diagnostics = 1 << 7,
        Caching = 1 << 8,
        Metalama = 1 << 9,

        All = int.MaxValue
    }
}