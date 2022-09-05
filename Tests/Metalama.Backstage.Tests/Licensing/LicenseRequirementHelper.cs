﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Backstage.Licensing.Tests.Licensing
{
    internal static class LicenseRequirementHelper
    {
        public static LicenseRequirement GetRequirement( LicenseRequirementTestEnum requirement )
            => requirement switch
            {
                LicenseRequirementTestEnum.Free => LicenseRequirement.Free,
                LicenseRequirementTestEnum.Starter => LicenseRequirement.Starter,
                LicenseRequirementTestEnum.Professional => LicenseRequirement.Professional,
                LicenseRequirementTestEnum.Ultimate => LicenseRequirement.Ultimate,
                _ => throw new ArgumentException( nameof( requirement ) )
            };
    }
}