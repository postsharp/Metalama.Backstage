// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Backstage.Licensing.Tests.Registration
{
    internal class TestLicenseAutoRegistrar : ILicenseAutoRegistrar
    {
        public bool RegistrationAttempted { get; set; }

        public bool TryRegisterLicense()
        {
            var success = !this.RegistrationAttempted;
            this.RegistrationAttempted = true;
            return success;
        }
    }
}
