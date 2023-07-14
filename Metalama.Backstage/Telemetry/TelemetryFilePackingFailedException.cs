// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Telemetry;

internal sealed class TelemetryFilePackingFailedException : Exception
{
    public string File { get; }

    public TelemetryFilePackingFailedException( string message, string file, Exception reason )
        : base( message, reason )
    {
        this.File = file;
    }
}