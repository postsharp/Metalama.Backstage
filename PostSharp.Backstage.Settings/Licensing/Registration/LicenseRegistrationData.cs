// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Backstage.Licensing.Registration
{
    public class LicenseRegistrationData : IEquatable<LicenseRegistrationData>
    {
        public string UniqueId { get; }

        public string? LicenseId { get; }

        public string? Licensee { get; }

        public string Description { get; }

        public LicenseType LicenseType { get; internal set; }

        public DateTime? ValidFrom { get; }

        public DateTime? ValidTo { get; }

        public bool? Perpetual { get; }

        public DateTime? SubscriptionEndDate { get; }

        public LicenseRegistrationData(
            string uniqueId,
            string? licensee,
            string description,
            LicenseType licenseType,
            DateTime? validFrom,
            DateTime? validTo,
            bool? perpetual,
            DateTime? subscriptionEndDate )
        {
            this.UniqueId = uniqueId;
            this.Licensee = licensee;
            this.Description = description;
            this.LicenseType = licenseType;
            this.ValidFrom = validFrom;
            this.ValidTo = validTo;
            this.Perpetual = perpetual;
            this.SubscriptionEndDate = subscriptionEndDate;
        }

        public bool Equals( LicenseRegistrationData other )
        {
            return this.UniqueId == other.UniqueId &&
                   this.Licensee == other.Licensee &&
                   this.Description == other.Description &&
                   this.LicenseType == other.LicenseType &&
                   this.ValidFrom == other.ValidFrom &&
                   this.ValidTo == other.ValidTo &&
                   this.Perpetual == other.Perpetual &&
                   this.SubscriptionEndDate == other.SubscriptionEndDate;
        }

        public override bool Equals( object? obj )
        {
            return obj is LicenseRegistrationData data &&
                   this.Equals( data );
        }

        public override int GetHashCode()
        {
            var hashCode = -1677246439;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode( this.UniqueId );
            hashCode = (hashCode * -1521134295) + EqualityComparer<string?>.Default.GetHashCode( this.Licensee );
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode( this.Description );
            hashCode = (hashCode * -1521134295) + this.LicenseType.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.ValidFrom.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.ValidTo.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.Perpetual.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.SubscriptionEndDate.GetHashCode();
            return hashCode;
        }
    }
}
