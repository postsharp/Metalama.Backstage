// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Extensibility;

namespace Metalama.Backstage.Telemetry;

public interface IUsageSample : IBackstageService
{
    MetricCollection Metrics { get; }

    void Flush();
}