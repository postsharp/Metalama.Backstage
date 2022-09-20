// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Newtonsoft.Json;
using System;

namespace Metalama.Backstage.Maintenance;

[ConfigurationFile( "cleanup.json" )]
public record CleanUpConfiguration : ConfigurationFile
{
    [JsonProperty( "LastCleanUpTime" )]
    public DateTime? LastCleanUpTime { get; init; }
}