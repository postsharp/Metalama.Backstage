// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Backstage.Testing.Services
{
    /// <inheritdoc />
    public class ApplicationInfo : IApplicationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInfo"/> class.
        /// </summary>
        /// <param name="isPrerelease">A value indicating whether the application is a pre-release.</param>
        /// <param name="version">The version of the application.</param>
        /// <param name="buildDate">The date of build of the application.</param>
        public ApplicationInfo( bool isPrerelease, Version version, DateTime buildDate )
        {
            this.IsPrerelease = isPrerelease;
            this.Version = version;
            this.BuildDate = buildDate;
        }

        /// <inheritdoc />
        public bool IsPrerelease { get; }

        /// <inheritdoc />
        public Version Version { get; }

        /// <inheritdoc />
        public DateTime BuildDate { get; }
    }
}
