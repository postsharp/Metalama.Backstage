// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Runtime.Serialization;

namespace PostSharp.Backstage.Licensing.Licenses
{
    /// <summary>
    /// Exception thrown when an invalid license is provided.
    /// </summary>
    [Serializable]
    public class InvalidLicenseException : Exception
    {
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

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

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLicenseException"/> class.
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected InvalidLicenseException(
            SerializationInfo info,
            StreamingContext context ) : base( info, context )
        {
        }
    }
}