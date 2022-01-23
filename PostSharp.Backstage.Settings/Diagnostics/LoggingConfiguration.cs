using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Diagnostics;

public class LoggingConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled at all.
    /// </summary>
    [JsonProperty( "processes" )]
    public Dictionary<ProcessKind, bool> Processes { get; set; } = new Dictionary<ProcessKind, bool>();

    /// <summary>
    /// Gets or sets the list of categories that are enabled for trace-level logging.
    /// </summary>
    [JsonProperty( "categories" )]

    public Dictionary<string, bool> Categories { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
}