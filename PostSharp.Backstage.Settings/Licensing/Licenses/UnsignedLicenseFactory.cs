// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration;
using PostSharp.Backstage.Licensing.Registration.Evaluation;

namespace PostSharp.Backstage.Licensing.Licenses
{
    /// <summary>
    /// Creates unsigned licenses for self-registration.
    /// </summary>
    internal class UnsignedLicenseFactory
    {
        private readonly IDateTimeProvider _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsignedLicenseFactory"/> class.
        /// </summary>
        /// <param name="services">Services.</param>
        public UnsignedLicenseFactory( IServiceProvider services )
        {
            this._time = services.GetRequiredService<IDateTimeProvider>();
        }

        /// <summary>
        /// Creates an unsigned evaluation license.
        /// </summary>
        /// <returns>The unsigned evaluation license.</returns>
        public (string LicenseKey, LicenseRegistrationData Data) CreateEvaluationLicense()
        {
            var start = this._time.Now.Date;
            var end = start + EvaluationLicenseRegistrar.EvaluationPeriod;

            var licenseKeyData = new LicenseKeyData
            {
                MinPostSharpVersion = LicenseKeyData.MinPostSharpVersionValidationRemovedPostSharpVersion,
                LicenseGuid = Guid.NewGuid(),
                Product = LicensedProduct.Caravela,
                LicenseType = LicenseType.Evaluation,
                ValidFrom = start,
                ValidTo = end,
                SubscriptionEndDate = end,
            };

            var licenseKey = licenseKeyData.Serialize();
            var licenseRegistrationData = licenseKeyData.ToRegistrationData();

            return (licenseKey, licenseRegistrationData);
        }

        /// <summary>
        /// Creates an unsigned community license.
        /// </summary>
        /// <returns>The unsigned community license.</returns>
        public (string LicenseKey, LicenseRegistrationData Data) CreateCommunityLicense()
        {
            var start = this._time.Now;

            var licenseKeyData = new LicenseKeyData
            {
                MinPostSharpVersion = LicenseKeyData.MinPostSharpVersionValidationRemovedPostSharpVersion,
                LicenseGuid = Guid.NewGuid(),
                Product = LicensedProduct.Caravela,
                LicenseType = LicenseType.Community,
                ValidFrom = start,
            };

            var licenseKey = licenseKeyData.Serialize();
            var licenseRegistrationData = licenseKeyData.ToRegistrationData();

            return (licenseKey, licenseRegistrationData);
        }
    }
}
