// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Licensing.Licenses
{
    /// <summary>
    /// Exception thrown when an invalid license is provided.
    /// </summary>
    public class InvalidLicenseException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLicenseException"/> class.
        /// </summary>
        public InvalidLicenseException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLicenseException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public InvalidLicenseException( string message ) : base( message )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLicenseException"/> class.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public InvalidLicenseException( string message, Exception inner ) : base( message, inner )
        {
        }
    }
}