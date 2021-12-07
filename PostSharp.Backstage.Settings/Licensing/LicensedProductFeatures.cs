// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing
{
    internal static class LicensedProductFeatures
    {
        public const LicensedFeatures Community = LicensedFeatures.Community | LicensedFeatures.Common;

        public const LicensedFeatures Mvvm = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Model |
                                             LicensedFeatures.Xaml | LicensedFeatures.Aggregation;

        public const LicensedFeatures Threading = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Threading
                                                  | LicensedFeatures.Aggregation;

        public const LicensedFeatures Logging = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Diagnostics;
        public const LicensedFeatures Caching = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Caching;
        public const LicensedFeatures Framework = LicensedFeatures.Community | LicensedFeatures.Common | LicensedFeatures.Framework | LicensedFeatures.Metalama;
        public const LicensedFeatures Ultimate = LicensedFeatures.All;
        public const LicensedFeatures Metalama = LicensedFeatures.Community | LicensedFeatures.Metalama;
    }
}