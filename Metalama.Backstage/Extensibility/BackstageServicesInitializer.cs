// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Backstage.Telemetry;
using Metalama.Backstage.UserInterface;
using Metalama.Backstage.Welcome;
using System;

namespace Metalama.Backstage.Extensibility;

internal sealed class BackstageServicesInitializer : IBackstageService
{
    private readonly IServiceProvider _serviceProvider;

    public BackstageServicesInitializer( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }

    public void Initialize()
    {
        this._serviceProvider.GetBackstageService<WelcomeService>()?.Initialize();

        this._serviceProvider.GetBackstageService<ProfilingService>()?.Initialize();

        // The license manager may enqueue a file but be unable to start the process.
        this._serviceProvider.GetBackstageService<ITelemetryUploader>()?.StartUpload();

        this._serviceProvider.GetBackstageService<ILicenseConsumptionService>()?.Initialize();

        // This should run after LicenseConsumptionService.
        this._serviceProvider.GetBackstageService<IUserInterfaceService>()?.Initialize();
    }
}