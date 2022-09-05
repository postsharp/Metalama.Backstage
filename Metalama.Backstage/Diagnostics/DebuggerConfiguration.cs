// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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