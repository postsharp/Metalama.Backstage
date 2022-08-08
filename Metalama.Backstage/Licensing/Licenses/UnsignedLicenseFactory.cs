// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Registration;
using Metalama.Backstage.Licensing.Registration.Evaluation;
using System;

namespace Metalama.Backstage.Licensing.Licenses
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
            this._time = services.GetRequiredBackstageService<IDateTimeProvider>();
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
                LicenseGuid = Guid.NewGuid(),
                Product = LicensedProduct.MetalamaUltimate,
                LicenseType = LicenseType.Evaluation,
                ValidFrom = start,
                ValidTo = end,
                SubscriptionEndDate = end
            };

            var licenseKey = licenseKeyData.Serialize();
            var licenseRegistrationData = licenseKeyData.ToRegistrationData();

            return (licenseKey, licenseRegistrationData);
        }

        /// <summary>
        /// Creates an unsigned Essentials license.
        /// </summary>
        /// <returns>The unsigned Essentials license.</returns>
        public (string LicenseKey, LicenseRegistrationData Data) CreateEssentialsLicense()
        {
            var start = this._time.Now;

            var licenseKeyData = new LicenseKeyData
            {
                LicenseGuid = Guid.NewGuid(), Product = LicensedProduct.MetalamaUltimate, LicenseType = LicenseType.Essentials, ValidFrom = start
            };

            var licenseKey = licenseKeyData.Serialize();
            var licenseRegistrationData = licenseKeyData.ToRegistrationData();

            return (licenseKey, licenseRegistrationData);
        }
    }
}