// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption;
using System;

namespace Metalama.Backstage.Extensibility;

/// <summary>
/// Initialization options for the <see cref="RegisterServiceExtensions.AddBackstageServices"/> method.
/// </summary>
/// <param name="ApplicationInfo">The <see cref="IApplicationInfo"/> of the caller.</param>
/// <param name="ProjectName">The project name, if relevant in the context. This is used to create more relevant log files.</param>
[PublicAPI]
public record BackstageInitializationOptions( IApplicationInfo ApplicationInfo, string? ProjectName = null )
{
    /// <summary>
    /// Gets the full path of the .NET SDK directory of the current process.
    /// </summary>
    public string? DotNetSdkDirectory { get; init; }

    /// <summary>
    /// Gets a value indicating whether the Welcome web page should be opened upon the first run of Metalama.
    /// </summary>
    public bool OpenWelcomePage { get; init; }

    /// <summary>
    /// Gets a value indicating whether logging and telemetry services should be registered.
    /// </summary>
    public bool AddSupportServices { get; init; }

    /// <summary>
    /// Gets a value indicating whether licensing services should be registered.
    /// </summary>
    public bool AddLicensing { get; init; }

    /// <summary>
    /// Gets the licensing options, when <see cref="AddLicensing"/> is <c>true</c>.
    /// </summary>
    public LicensingInitializationOptions LicensingOptions { get; init; } = new();

    /// <summary>
    /// Gets an optional action that registers the <see cref="ILoggerFactory"/>. Considered only when <see cref="AddSupportServices"/> is <c>true</c>.
    /// </summary>
    public Func<IServiceProvider, ILoggerFactory>? CreateLoggingFactory { get; init; }
}