// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;
using System;
using System.Collections.Immutable;

namespace Metalama.Backstage.Diagnostics;

public record MiniDumpConfiguration
{
    /// <summary>
    /// Gets a value indicating whether logging is enabled at all.
    /// </summary>
    [JsonProperty( "processes" )]
    public ImmutableDictionary<string, bool> Processes { get; init; } =
        ImmutableDictionary<string, bool>.Empty.WithComparers( StringComparer.OrdinalIgnoreCase );

    [JsonProperty( "flags" )]
    public ImmutableArray<string> Flags { get; init; } = ImmutableArray<string>.Empty;

    [JsonProperty( "exceptionTypes" )]
    public ImmutableArray<string> ExceptionTypes { get; init; } = ImmutableArray<string>.Empty;
}