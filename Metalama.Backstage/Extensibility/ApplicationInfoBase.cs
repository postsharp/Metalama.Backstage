// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Backstage.Extensibility
{
    /// <summary>
    /// Implementation of <see cref="IApplicationInfo" /> interface with build information
    /// initialized from assembly metadata using <see cref="AssemblyMetadataReader" />.
    /// </summary>
    public abstract class ApplicationInfoBase : IApplicationInfo
    {
        protected ApplicationInfoBase( Assembly metadataAssembly )
        {
            var reader = AssemblyMetadataReader.GetInstance( metadataAssembly );
            this.Version = reader.PackageVersion;
#pragma warning disable CA1307
            this.IsPrerelease = this.Version?.Contains( "-" );
#pragma warning restore CA1307
            this.BuildDate = reader.BuildDate;
            this.Company = reader.Company;

            if ( this.Version != null )
            {
                var versionParts = this.Version.Split( '-' );
                this.IsTelemetryEnabled = versionParts.Length == 1 || versionParts[1] is not ("dev" or "local");
            }
        }

        public string? Company { get; }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public string? Version { get; }

        /// <inheritdoc />
        public bool? IsPrerelease { get; }

        /// <inheritdoc />
        public DateTime? BuildDate { get; }

        /// <inheritdoc />
        public virtual ProcessKind ProcessKind => ProcessUtilities.ProcessKind;

        /// <inheritdoc />
        public virtual bool IsLongRunningProcess => false;

        /// <inheritdoc />
        public virtual bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => ProcessUtilities.IsCurrentProcessUnattended( loggerFactory );

        /// <inheritdoc />
        public virtual bool IsTelemetryEnabled { get; }

        /// <inheritdoc />
        public virtual bool ShouldCreateLocalCrashReports => true;

        public virtual ImmutableArray<IComponentInfo> Components => ImmutableArray<IComponentInfo>.Empty;
    }
}