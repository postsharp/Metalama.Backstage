﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Diagnostics;
using PostSharp.Backstage.Extensibility;
using System;

namespace PostSharp.Backstage.Testing.Services
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
        public TestApplicationInfo( string name, bool isPrerelease, string version, DateTime buildDate )
        {
            this.Name = name;
            this.IsPrerelease = isPrerelease;
            this.Version = version;
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

        public bool IsLongRunningProcess => false;
    }
}