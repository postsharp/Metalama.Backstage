// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Maintenance;
using Newtonsoft.Json;
using System;

namespace Metalama.Backstage.Licensing.Tests.Maintenance;

[ConfigurationFile( "cleanup.json" )]
public class TestCleanUpConfiguration : CleanUpConfiguration
{
    [JsonProperty( "LastCleanUpTime" )]
    public new DateTime? LastCleanUpTime { get; set; }

    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (CleanUpConfiguration) configurationFile;
        this.LastCleanUpTime = source.LastCleanUpTime;
    }

    public void SetLastCleanUpTime( DateTime lastCleanUpTime ) => this.LastCleanUpTime = lastCleanUpTime;

}