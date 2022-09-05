// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Newtonsoft.Json;
using System;

namespace Metalama.Backstage.Maintenance;

[ConfigurationFile( "cleanup.json" )]
public class CleanUpConfiguration : ConfigurationFile
{
    [JsonProperty( "LastCleanUpTime" )]
    public DateTime? LastCleanUpTime { get; private set; }

    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (CleanUpConfiguration) configurationFile;
        this.LastCleanUpTime = source.LastCleanUpTime;
    }

    public void ResetLastCleanUpTime() => this.LastCleanUpTime = DateTime.Now;
}