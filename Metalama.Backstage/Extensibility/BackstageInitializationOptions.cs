// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Application;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.UserInterface;
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
    /// Gets a value indicating whether logging and telemetry services should be registered.
    /// </summary>
    public bool AddSupportServices { get; init; }

    /// <summary>
    /// Gets a value indicating whether licensing services should be registered.
    /// </summary>
    public bool AddLicensing { get; init; }

    public bool AddUserInterface { get; init; }

    /// <summary>
    /// Gets a value indicating whether the current program executes from a development environment,
    /// i.e. tools are located under the bin directory of their respective projects. 
    /// </summary>
    public bool IsDevelopmentEnvironment { get; init; }

    public Action<ServiceProviderBuilder>? AddToolsExtractor { get; init; }

    /// <summary>
    /// Gets the licensing options, when <see cref="AddLicensing"/> is <c>true</c>.
    /// </summary>
    public LicensingInitializationOptions LicensingOptions { get; init; } = new();

    /// <summary>
    /// Gets an optional action that registers the <see cref="ILoggerFactory"/>. Considered only when <see cref="AddSupportServices"/> is <c>true</c>.
    /// </summary>
    public Func<IServiceProvider, ILoggerFactory>? CreateLoggingFactory { get; init; }

    /// <summary>
    /// Gets a value indicating whether the services should be initialized. The default value is <c>true</c>.
    /// It can be set to <c>false</c> in scenarios where it is not necessary to build up the whole application
    /// because just a few services will be used.
    /// </summary>
    public bool Initialize { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether toast notifications like <see cref="ToastNotificationKinds.RequiresLicense"/> or
    /// <see cref="ToastNotificationKinds.VsxNotInstalled"/> should be detected and opened. The default value is <c>true</c>.
    /// </summary>
    public bool DetectToastNotifications { get; init; } = true;
}