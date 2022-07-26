// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Metalama.Backstage.Diagnostics;

public class DebuggerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled at all.
    /// </summary>
    [JsonProperty( "processes" )]
    public Dictionary<ProcessKind, bool> Processes { get; set; } = new();

    public DebuggerConfiguration Clone()
    {
        return new DebuggerConfiguration() { Processes = new Dictionary<ProcessKind, bool>( this.Processes ) };
    }
}