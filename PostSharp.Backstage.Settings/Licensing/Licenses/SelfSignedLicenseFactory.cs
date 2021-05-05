// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Evaluation;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Licenses
{
    internal class SelfSignedLicenseFactory
    {
        private readonly IDateTimeProvider _time;

        public SelfSignedLicenseFactory( IServiceProvider services )
        {
            this._time = services.GetService<IDateTimeProvider>();
        }

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
