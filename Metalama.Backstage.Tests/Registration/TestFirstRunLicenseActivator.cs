// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Licensing.Registration;
using System;

namespace Metalama.Backstage.Licensing.Tests.Registration
{
    internal class TestFirstRunLicenseActivator : IFirstRunLicenseActivator
    {
        public bool RegistrationAttempted { get; set; }

        public bool TryActivateLicense( Action<LicensingMessage> reportMessage )
        {
            this.RegistrationAttempted = true;

            return false;
        }
    }
}