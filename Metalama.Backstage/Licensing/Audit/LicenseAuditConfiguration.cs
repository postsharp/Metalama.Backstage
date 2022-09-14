// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using System;
using System.Collections.Immutable;

namespace Metalama.Backstage.Licensing.Audit;

[ConfigurationFile( "audit.json" )]
public class LicenseAuditConfiguration : ConfigurationFile
{
    public ImmutableDictionary<int, DateTime> LastAuditTimes { get; set; } =
        ImmutableDictionary<int, DateTime>.Empty;
    
    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (LicenseAuditConfiguration) configurationFile;
        this.LastAuditTimes = source.LastAuditTimes;
    }
}