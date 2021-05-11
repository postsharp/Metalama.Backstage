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
        Aggregatable = 1 << 6,
        Diagnostics = 1 << 7,
        Caching = 1 << 8,
        Caravela = 1 << 9,

        All = int.MaxValue,
    }

#pragma warning disable SA1649 // File name should match first type name
    internal static class LicensedProductFeatures
#pragma warning restore SA1649 // File name should match first type name
    {
        public const LicensedFeatures Community = LicensedFeatures.Community | LicensedFeatures.Common;
        public const LicensedFeatures Mvvm = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Model |
                                             LicensedFeatures.Xaml | LicensedFeatures.Aggregatable;

        public const LicensedFeatures Threading = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Threading | LicensedFeatures.Aggregatable;
        public const LicensedFeatures Logging = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Diagnostics;
        public const LicensedFeatures Caching = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Caching;
        public const LicensedFeatures Framework = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Framework | LicensedFeatures.Caravela;
        public const LicensedFeatures Ultimate = LicensedFeatures.All;
        public const LicensedFeatures Unattended = LicensedFeatures.All & ~LicensedFeatures.Diagnostics;
        public const LicensedFeatures Caravela = LicensedFeatures.Caravela;
    }
}
