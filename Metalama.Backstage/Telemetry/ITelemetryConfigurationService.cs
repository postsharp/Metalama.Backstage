// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.Telemetry;

[PublicAPI]
public interface ITelemetryConfigurationService : IBackstageService
{
    bool IsEnabled { get; }
}