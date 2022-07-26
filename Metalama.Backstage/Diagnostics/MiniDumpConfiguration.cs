// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Metalama.Backstage.Diagnostics;

public class MiniDumpConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled at all.
    /// </summary>
    [JsonProperty( "processes" )]
    public Dictionary<ProcessKind, bool> Processes { get; set; } = new();

    [JsonProperty( "flags" )]
    public List<string> Flags { get; set; } = new();

    public List<string> ExceptionTypes { get; set; } = new();
}