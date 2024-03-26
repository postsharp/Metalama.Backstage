// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using System;

namespace Metalama.Backstage.Telemetry;

public interface ITelemetryConfigurationService : IBackstageService
{
    void SetStatus( bool enabled );

    Guid DeviceId { get; }

    void ResetDeviceId();
}