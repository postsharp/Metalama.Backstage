// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using PostSharp.Backstage.Licensing.Licenses;

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Information about a license relevant to license registration.
    /// </summary>
    /// <remarks>
    /// Properties returning a null value are not intended to be presented to a user.
    /// </remarks>
    public class LicenseRegistrationData : IEquatable<LicenseRegistrationData>
    {
        /// <summary>
        /// Gets a unique identifier of the license.
        /// </summary>
        public string UniqueId { get; }

        /// <summary>
        /// Gets the license identifier of a non-self-signed license key.
        /// </summary>
        public int? LicenseId { get; }

        /// <summary>
        /// Gets the licensee.
        /// </summary>
        public string? Licensee { get; }

        /// <summary>
        /// Gets a description of the license.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the type of the license.
        /// </summary>
        public LicenseType LicenseType { get; internal set; }

        /// <summary>
        /// Gets the beginning of the license validity.
        /// </summary>
        public DateTime? ValidFrom { get; }

        /// <summary>
        /// Gets the end of the license validity.
        /// </summary>
        /// <remarks>
        /// Null value is returned when a validity is irrelevant for the license.
        /// A perpetual license is identified by <see cref="Perpetual" /> property.
        /// </remarks>
        public DateTime? ValidTo { get; }

        /// <summary>
        /// Gets a value indicating whether the license is perpetual.
        /// </summary>
        public bool? Perpetual { get; }

        /// <summary>
        /// Gets the expiration date of the maintenance subscription.
        /// </summary>
        public DateTime? SubscriptionEndDate { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseRegistrationData"/> class.
        /// </summary>
        /// <remarks>
        /// Null values are not intended to be presented to a user.
        /// </remarks>
        /// <param name="uniqueId">The unique identifier of the license.</param>
        /// <param name="licensee">The licensee.</param>
        /// <param name="description">The description of the license.</param>
        /// <param name="licenseType">The type of the license.</param>
        /// <param name="validFrom">The beginning of the license validity.</param>
        /// <param name="validTo">The end of the license validity.</param>
        /// <param name="perpetual">A value indicating whether the license is perpetual.</param>
        /// <param name="subscriptionEndDate">The expiration date of the maintenance subscription.</param>
        public LicenseRegistrationData(
            string uniqueId,
            int? licenseId,
            string? licensee,
            string description,
            LicenseType licenseType,
            DateTime? validFrom,
            DateTime? validTo,
            bool? perpetual,
            DateTime? subscriptionEndDate )
        {
            this.UniqueId = uniqueId;
            this.LicenseId = licenseId;
            this.Licensee = licensee;
            this.Description = description;
            this.LicenseType = licenseType;
            this.ValidFrom = validFrom;
            this.ValidTo = validTo;
            this.Perpetual = perpetual;
            this.SubscriptionEndDate = subscriptionEndDate;
        }

        /// <inheritdoc />
        public bool Equals( LicenseRegistrationData other )
        {
            return this.UniqueId == other.UniqueId &&
                   this.LicenseId == other.LicenseId &&
                   this.Licensee == other.Licensee &&
                   this.Description == other.Description &&
                   this.LicenseType == other.LicenseType &&
                   this.ValidFrom == other.ValidFrom &&
                   this.ValidTo == other.ValidTo &&
                   this.Perpetual == other.Perpetual &&
                   this.SubscriptionEndDate == other.SubscriptionEndDate;
        }

        /// <inheritdoc />
        public override bool Equals( object? obj )
        {
            return obj is LicenseRegistrationData data &&
                   this.Equals( data );
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = -1677246439;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode( this.UniqueId );
            hashCode = (hashCode * -1521134295) + EqualityComparer<int?>.Default.GetHashCode( this.LicenseId );
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
