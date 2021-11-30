// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace PostSharp.Backstage.Licensing.Registration
{
    /// <summary>
    /// Registers a license without user interaction. This interface is used the first time PostSharp is launched when no license is present.
    /// </summary>
    public interface IFirstRunLicenseActivator
    {
        /// <summary>
        /// Tries to register a license without user interaction.
        /// </summary>
        /// <returns>A value indicating whether a license has been registered.</returns>
        bool TryRegisterLicense();
    }
}