// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Backstage.Extensibility
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
        Version Version { get; }

        /// <summary>
        /// Gets a value indicating whether the application is a pre-release.
        /// </summary>
        bool IsPrerelease { get; }

        /// <summary>
        /// Gets a date of build of the application.
        /// </summary>
        DateTime BuildDate { get; }
    }
}