﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

namespace Metalama.Backstage.Licensing
{
    internal static class LicensedProductFeatures
    {
        public const LicensedFeatures Essentials = LicensedFeatures.Essentials | LicensedFeatures.Common |
            LicensedFeatures.MetalamaAspects |
            LicensedFeatures.MetalamaFabricsConfiguration;

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
                                                  LicensedFeatures.Framework | LicensedFeatures.MetalamaAspects;

        public const LicensedFeatures Ultimate = LicensedFeatures.All;

        public const LicensedFeatures MetalamaStarter =
            LicensedFeatures.Essentials |
            LicensedFeatures.MetalamaAspects |
            LicensedFeatures.MetalamaFabricsConfiguration |
            LicensedFeatures.MetalamaFabricsAspects |
            LicensedFeatures.MetalamaFabricsValidators |
            LicensedFeatures.MetalamaExportTransformedCode |
            LicensedFeatures.MetalamaDebugTransformedCode |
            LicensedFeatures.MetalamaAspectInheritance;

        public const LicensedFeatures MetalamaProfessional =
            LicensedFeatures.Essentials |
            LicensedFeatures.MetalamaAspects |
            LicensedFeatures.MetalamaFabricsConfiguration |
            LicensedFeatures.MetalamaFabricsAspects |
            LicensedFeatures.MetalamaFabricsValidators |
            LicensedFeatures.MetalamaExportTransformedCode |
            LicensedFeatures.MetalamaDebugTransformedCode |
            LicensedFeatures.MetalamaTestFramework |
            LicensedFeatures.MetalamaSdk |
            LicensedFeatures.MetalamaQuickActions |
            LicensedFeatures.MetalamaAspectInheritance;

        public const LicensedFeatures MetalamaUltimate =
            LicensedFeatures.Essentials |
            LicensedFeatures.MetalamaAspects |
            LicensedFeatures.MetalamaFabricsConfiguration |
            LicensedFeatures.MetalamaFabricsAspects |
            LicensedFeatures.MetalamaFabricsValidators |
            LicensedFeatures.MetalamaExportTransformedCode |
            LicensedFeatures.MetalamaDebugTransformedCode |
            LicensedFeatures.MetalamaTestFramework |
            LicensedFeatures.MetalamaSdk |
            LicensedFeatures.MetalamaQuickActions |
            LicensedFeatures.MetalamaAspectInheritance;
    }
}