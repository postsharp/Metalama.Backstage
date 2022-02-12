// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using System;

namespace Metalama.Backstage.Extensibility
{
    /// <summary>
    /// Provides version information about an application.
    /// </summary>
    public interface IApplicationInfo
    {
        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a version of the application.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets a value indicating whether the application is a pre-release.
        /// </summary>
        bool IsPrerelease { get; }

        /// <summary>
        /// Gets a date of build of the application.
        /// </summary>
        DateTime BuildDate { get; }

        ProcessKind ProcessKind { get; }

        /// <summary>
        /// Determines whether the current process is unattended. This method accepts an <see cref="ILoggerFactory"/> because
        /// the <see cref="IServiceProvider"/> cannot be available when we instantiate implementations of this interface.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        bool IsUnattendedProcess( ILoggerFactory loggerFactory );

        bool IsLongRunningProcess { get; }
    }
}