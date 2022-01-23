using Newtonsoft.Json;
using System.Collections.Generic;

namespace PostSharp.Backstage.Diagnostics;

public class DebuggerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled at all.
    /// </summary>
    [JsonProperty( "processes" )]
    public Dictionary<ProcessKind, bool> Processes { get; set; } = new Dictionary<ProcessKind, bool>();
}