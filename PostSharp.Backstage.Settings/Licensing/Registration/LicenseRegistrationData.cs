// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Backstage.Licensing.Registration
{
    public class LicenseRegistrationData
    {
        public string UniqueId { get; }

        public string? Licensee { get; }

        public string Description { get; }

        public LicenseType LicenseType { get; internal set; }

        public DateTime? ValidFrom { get; }

        public DateTime? ValidTo { get; }

        public DateTime? SubscriptionEndDate { get; }

        public LicenseRegistrationData(
            string uniqueId,
            string? licensee,
            string description,
            LicenseType licenseType,
            DateTime? validFrom,
            DateTime? validTo,
            DateTime? subscriptionEndDate )
        {
            this.UniqueId = uniqueId;
            this.Licensee = licensee;
            this.Description = description;
            this.LicenseType = licenseType;
            this.ValidFrom = validFrom;
            this.ValidTo = validTo;
            this.SubscriptionEndDate = subscriptionEndDate;
        }
    }
}
