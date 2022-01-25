// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Configuration;
using PostSharp.Backstage.Extensibility;
using System;

namespace PostSharp.Backstage.Telemetry;

internal class UsageReporter : IUsageReporter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TelemetryConfiguration _configuration;
    private readonly UploadManager _uploadManager;

    public UsageReporter( UploadManager uploadManager, IServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._configuration = serviceProvider.GetRequiredService<IConfigurationManager>().Get<TelemetryConfiguration>();
        this._uploadManager = uploadManager;
    }

    public IUsageSample CreateSample( string kind )
    {
        return new UsageSample( this._serviceProvider, this._configuration, kind, this._uploadManager );
    }
}