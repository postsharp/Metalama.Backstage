// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Licenses;
using System;

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
            LicenseType licenseType,
            DateTime? validFrom,
            DateTime? validTo,
            bool? perpetual,
            DateTime? subscriptionEndDate )
        {
            UniqueId = uniqueId;
            IsSelfCreated = isSelfCreated;
            LicenseId = licenseId;
            Licensee = licensee;
            Description = description;
            LicenseType = licenseType;
            ValidFrom = validFrom;
            ValidTo = validTo;
            Perpetual = perpetual;
            SubscriptionEndDate = subscriptionEndDate;
        }

        /// <inheritdoc />
        public bool Equals( LicenseRegistrationData other )
        {
            return UniqueId == other.UniqueId &&
                   IsSelfCreated == other.IsSelfCreated &&
                   LicenseId == other.LicenseId &&
                   Licensee == other.Licensee &&
                   Description == other.Description &&
                   LicenseType == other.LicenseType &&
                   ValidFrom == other.ValidFrom &&
                   ValidTo == other.ValidTo &&
                   Perpetual == other.Perpetual &&
                   SubscriptionEndDate == other.SubscriptionEndDate;
        }

        /// <inheritdoc />
        public override bool Equals( object? obj )
        {
            return obj is LicenseRegistrationData data &&
                   Equals( data );
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            HashCode hashCode = default;
            hashCode.Add( UniqueId );
            hashCode.Add( IsSelfCreated );
            hashCode.Add( LicenseId );
            hashCode.Add( Licensee );
            hashCode.Add( Description );
            hashCode.Add( LicenseType );
            hashCode.Add( ValidFrom );
            hashCode.Add( ValidTo );
            hashCode.Add( Perpetual );
            hashCode.Add( SubscriptionEndDate );

            return hashCode.ToHashCode();
        }
    }
}