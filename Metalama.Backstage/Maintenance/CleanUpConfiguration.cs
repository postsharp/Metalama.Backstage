// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Metalama.Backstage.Maintenance;

[ConfigurationFile( "cleanup.json" )]
public class CleanUpConfiguration : ConfigurationFile
{
    [JsonProperty( "lastCleanUp" )]
    public DateTime LastCleanUp { get; private set; }

    public CleanUpConfiguration()
    {
        this.Reset();
    }

    // TODO: Do we need this
    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (CleanUpConfiguration) configurationFile;
        this.LastCleanUp = source.Clone();
    }

    public void Reset()
    {
        this.LastCleanUp = DateTime.Now;
    }

    public new DateTime Clone() => this.LastCleanUp;
}