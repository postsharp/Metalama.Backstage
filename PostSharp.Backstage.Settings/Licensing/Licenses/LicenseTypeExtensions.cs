﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing.Licenses
{
    public static class LicenseTypeExtensions
    {
        public static string GetLicenseTypeName( this LicenseType licenseType )
        {
#pragma warning disable 618
            switch ( licenseType )
            {
                case LicenseType.Community:
                    // This case is handled in CoreLicense.GetProductName().
                    return "License";
                case LicenseType.Enterprise:
                case LicenseType.PerUser:
                    return "Per-Developer Subscription";
                case LicenseType.Site:
                    return "Site License";
                case LicenseType.Global:
                    return "Global License";
                case LicenseType.Evaluation:
                    return "Evaluation License";
                case LicenseType.Academic:
                    return "Academic License";
                case LicenseType.CommercialRedistribution:
                    return "Commercial Redistribution License";
                case LicenseType.PerUsage:
                    return "Per-Usage Subscription";
#pragma warning disable 618
                case LicenseType.Anonymous:
                    return "Anonymous License";
#pragma warning restore 618
                default:

                    // We don't want to display the license type for other licenses, because there may be
                    // a mismatch between what we sell (i.e. what is represented in the CRM and in the license certificate)
                    // and what is serialized into the license key.
                    return "License";
            }
#pragma warning restore 618
        }
    }
}