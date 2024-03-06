// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Telemetry;
using Metalama.Backstage.UserInterface;
using Metalama.Backstage.Welcome;
using System;

namespace Metalama.Backstage.Extensibility;

internal sealed class BackstageServicesInitializer : IBackstageService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly BackstageInitializationOptionsProvider _optionsProvider;
    private readonly BackstageBackgroundTasksService _backgroundTasksService;

    public BackstageServicesInitializer( IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._optionsProvider = serviceProvider.GetRequiredBackstageService<BackstageInitializationOptionsProvider>();
        this._backgroundTasksService = serviceProvider.GetRequiredBackstageService<BackstageBackgroundTasksService>();
    }

    public void Initialize()
    {
        this._serviceProvider.GetBackstageService<WelcomeService>()?.Initialize();

        this._serviceProvider.GetBackstageService<IProfilingService>()?.Initialize();

        this._backgroundTasksService.Enqueue(
            () =>
            {
                // The license manager may enqueue a file but be unable to start the process.
                this._serviceProvider.GetBackstageService<ITelemetryUploader>()?.StartUpload();
                this._serviceProvider.GetBackstageService<ToastNotificationDetectionService>()?.Detect();
            } );
    }
}