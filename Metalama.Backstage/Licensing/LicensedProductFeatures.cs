﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Backstage.Licensing
{
    internal static class LicensedProductFeatures
    {
        public const LicensedFeatures Essentials = LicensedFeatures.Essentials | LicensedFeatures.Common;

        public const LicensedFeatures Mvvm = LicensedFeatures.Essentials | LicensedFeatures.Common |
                                             LicensedFeatures.Model |
                                             LicensedFeatures.Xaml | LicensedFeatures.Aggregation;

        public const LicensedFeatures Threading = LicensedFeatures.Essentials | LicensedFeatures.Common |
                                                  LicensedFeatures.Threading
                                                  | LicensedFeatures.Aggregation;

        public const LicensedFeatures Logging =
            LicensedFeatures.Essentials | LicensedFeatures.Common | LicensedFeatures.Diagnostics;

        public const LicensedFeatures Caching =
            LicensedFeatures.Essentials | LicensedFeatures.Common | LicensedFeatures.Caching;

        public const LicensedFeatures Framework = LicensedFeatures.Essentials | LicensedFeatures.Common |
                                                  LicensedFeatures.Framework | LicensedFeatures.Metalama;

        public const LicensedFeatures Ultimate = LicensedFeatures.All;
        public const LicensedFeatures Metalama = LicensedFeatures.Essentials | LicensedFeatures.Metalama;
    }
}