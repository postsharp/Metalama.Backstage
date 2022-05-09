// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
        public TestApplicationInfo( string name, bool isPrerelease, string version, DateTime buildDate, bool isUnattendedProcess = false )
        {
            this.Name = name;
            this.IsPrerelease = isPrerelease;
            this.Version = version;
            this._isUnattendedProcess = isUnattendedProcess;
            this.BuildDate = buildDate;
        }

        public string Name { get; }

        /// <inheritdoc />
        public bool IsPrerelease { get; }

        /// <inheritdoc />
        public string Version { get; }

        /// <inheritdoc />
        public DateTime BuildDate { get; }

        public ProcessKind ProcessKind => ProcessKind.Other;

        public bool IsUnattendedProcess( ILoggerFactory loggerFactory ) => this._isUnattendedProcess;

        public bool IsLongRunningProcess => false;
    }
}