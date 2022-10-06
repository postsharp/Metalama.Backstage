// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Backstage.Testing.Services
{
    /// <inheritdoc />
    public class TestApplicationInfo : IApplicationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestApplicationInfo"/> class.
        /// </summary>
        /// <param name="isPrerelease">A value indicating whether the application is a pre-release.</param>
        /// <param name="version">The version of the application.</param>
        /// <param name="buildDate">The date of build of the application.</param>
        public TestApplicationInfo(
            string name,
            bool isPrerelease,
            string version,
            DateTime buildDate )
        {
            this.Name = name;
            this.IsPrerelease = isPrerelease;
            this.Version = version;
            this.BuildDate = buildDate;
            this.Company = AssemblyMetadataReader.GetInstance( typeof(TestApplicationInfo).Assembly ).Company;
        }

        public TestApplicationInfo() : this( "test", false, "0.0", DateTime.Now ) { }

        public string? Company { get; init; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool? IsPrerelease { get; init; }

        /// <inheritdoc />
        public string? Version { get; }

        /// <inheritdoc />
        public DateTime? BuildDate { get; }

        /// <inheritdoc />
        public ProcessKind ProcessKind => ProcessKind.Other;

        public bool IsUnattendedProcess { get; init; }

        /// <inheritdoc />
        bool IApplicationInfo.IsUnattendedProcess( ILoggerFactory loggerFactory ) => this.IsUnattendedProcess;

        /// <inheritdoc />
        public bool IsLongRunningProcess => false;

        /// <inheritdoc />
        public bool IsTelemetryEnabled { get; init; }

        /// <inheritdoc />
        public bool ShouldCreateLocalCrashReports => false;

        /// <inheritdoc />
        public ImmutableArray<IComponentInfo> Components { get; init; } = ImmutableArray<IComponentInfo>.Empty;
    }
}