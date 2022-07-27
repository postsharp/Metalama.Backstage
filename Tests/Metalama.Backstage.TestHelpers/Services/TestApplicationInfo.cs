// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Testing.Services
{
    /// <inheritdoc />
    public class TestApplicationInfo : IApplicationInfo
    {
        private readonly bool _isUnattendedProcess;

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
            DateTime buildDate,
            bool isUnattendedProcess = false,
            bool isTelemetryEnabled = false )
        {
            this.Name = name;
            this.IsPrerelease = isPrerelease;
            this.Version = version;
            this._isUnattendedProcess = isUnattendedProcess;
            this.BuildDate = buildDate;
            this.IsTelemetryEnabled = false;
        }

        public TestApplicationInfo() : this( "test", false, "0.0", DateTime.Now ) { }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public bool IsPrerelease { get; }

        /// <inheritdoc />
        public string Version { get; }

        /// <inheritdoc />
        public DateTime BuildDate { get; }

        /// <inheritdoc />
        public ProcessKind ProcessKind => ProcessKind.Other;

        /// <inheritdoc />
        public bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => this._isUnattendedProcess;

        /// <inheritdoc />
        public bool IsLongRunningProcess => false;

        /// <inheritdoc />
        public bool IsTelemetryEnabled { get; }
    }
}