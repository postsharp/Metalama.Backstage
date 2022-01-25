// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Licenses;
using System;

namespace Metalama.Backstage.Licensing.Registration
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
        /// Gets a value indicating whether the license is self-created.
        /// </summary>
        public bool IsSelfCreated { get; }

        /// <summary>
        /// Gets the license identifier of a not-self-created license key.
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
        /// Gets the product licensed by the license.
        /// </summary>
        public LicensedProduct Product { get; }

        /// <summary>
        /// Gets the type of the license.
        /// </summary>
        public LicenseType LicenseType { get; }

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
        /// Gets a value indicating whether the license usage can be audited.
        /// </summary>
        public bool Auditable { get; }

        /// <summary>
        /// Gets a value indicating whether the license can be installed on a license server.
        /// </summary>
        public bool LicenseServerEligible { get; }

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
            bool isSelfCreated,
            int? licenseId,
            string? licensee,
            string description,
            LicensedProduct product,
            LicenseType licenseType,
            DateTime? validFrom,
            DateTime? validTo,
            bool? perpetual,
            DateTime? subscriptionEndDate,
            bool auditable,
            bool licenseServerEligible )
        {
            this.UniqueId = uniqueId;
            this.IsSelfCreated = isSelfCreated;
            this.LicenseId = licenseId;
            this.Licensee = licensee;
            this.Description = description;
            this.LicenseType = licenseType;
            this.Product = product;
            this.ValidFrom = validFrom;
            this.ValidTo = validTo;
            this.Perpetual = perpetual;
            this.SubscriptionEndDate = subscriptionEndDate;
            this.Auditable = auditable;
            this.LicenseServerEligible = licenseServerEligible;
        }

        /// <inheritdoc />
        public bool Equals( LicenseRegistrationData other )
        {
            return this.UniqueId == other.UniqueId &&
                   this.IsSelfCreated == other.IsSelfCreated &&
                   this.LicenseId == other.LicenseId &&
                   this.Licensee == other.Licensee &&
                   this.Description == other.Description &&
                   this.LicenseType == other.LicenseType &&
                   this.ValidFrom == other.ValidFrom &&
                   this.ValidTo == other.ValidTo &&
                   this.Perpetual == other.Perpetual &&
                   this.SubscriptionEndDate == other.SubscriptionEndDate &&
                   this.Auditable == other.Auditable &&
                   this.LicenseServerEligible == other.LicenseServerEligible;
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
            HashCode hashCode = default;
            hashCode.Add( this.UniqueId );
            hashCode.Add( this.IsSelfCreated );
            hashCode.Add( this.LicenseId );
            hashCode.Add( this.Licensee );
            hashCode.Add( this.Description );
            hashCode.Add( this.LicenseType );
            hashCode.Add( this.ValidFrom );
            hashCode.Add( this.ValidTo );
            hashCode.Add( this.Perpetual );
            hashCode.Add( this.SubscriptionEndDate );
            hashCode.Add( this.Auditable );
            hashCode.Add( this.LicenseServerEligible );

            return hashCode.ToHashCode();
        }
    }
}