// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using System;

namespace Metalama.Backstage.Maintenance;

[ConfigurationFile( "cleanup.json" )]
public class CleanUpConfiguration : ConfigurationFile
{
    public DateTime? LastCleanUpTime { get; private set; }

    public CleanUpConfiguration()
    {
        this.ResetLastCleanUpTime();
    }

    // TODO: Do we need this
    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (CleanUpConfiguration) configurationFile;
        this.LastCleanUpTime = source.LastCleanUpTime;
    }

    public void ResetLastCleanUpTime()
    {
        this.LastCleanUpTime = DateTime.Now;
    }
}