// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using System;
using System.Reflection;

namespace Metalama.Backstage.Extensibility
{
    /// <summary>
    /// Implementation of <see cref="IApplicationInfo" /> interface with build information
    /// initialized from assembly metadata using <see cref="AssemblyMetadataReader" />.
    /// </summary>
    public abstract class ApplicationInfoBase : IApplicationInfo
    {
        public ApplicationInfoBase( Assembly metadataAssembly )
        {
            var reader = AssemblyMetadataReader.GetInstance( metadataAssembly );
            this.Version = reader.PackageVersion;
#pragma warning disable CA1307
            this.IsPrerelease = this.Version.Contains( "-" );
#pragma warning restore CA1307
            this.BuildDate = reader.BuildDate;
        }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public string Version { get; }

        /// <inheritdoc />
        public bool IsPrerelease { get; }

        /// <inheritdoc />
        public DateTime BuildDate { get; }

        /// <inheritdoc />
        public abstract ProcessKind ProcessKind { get; }

        /// <inheritdoc />
        public abstract bool IsLongRunningProcess { get; }

        /// <inheritdoc />
        public abstract bool IsUnattendedProcess( ILoggerFactory loggerFactory );

        /// <inheritdoc />
        public abstract bool IsTelemetryEnabled { get; }
    }
}