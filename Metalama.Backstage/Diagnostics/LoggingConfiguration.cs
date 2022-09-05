// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Diagnostics;

public class LoggingConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled at all.
    /// </summary>
    [JsonProperty( "processes" )]
    public Dictionary<ProcessKind, bool> Processes { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of categories that are enabled for trace-level logging.
    /// </summary>
    [JsonProperty( "categories" )]

    public Dictionary<string, bool> Categories { get; set; } = new( StringComparer.OrdinalIgnoreCase );

    public LoggingConfiguration Clone()
        => new() { Processes = new Dictionary<ProcessKind, bool>( this.Processes ), Categories = new Dictionary<string, bool>( this.Categories ) };
}