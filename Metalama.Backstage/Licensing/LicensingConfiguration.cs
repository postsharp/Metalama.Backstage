// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using Metalama.Backstage.Configuration;
using Newtonsoft.Json;
using System;

namespace Metalama.Backstage.Licensing;

[ConfigurationFile( "licensing.json" )]
internal class LicensingConfiguration : ConfigurationFile
{
    /// <summary>
    /// Gets or sets the date of the last evaluation period.
    /// </summary>
    [JsonProperty( "lastEvaluationStartDate" )]
    public DateTime? LastEvaluationStartDate { get; set; }

    [JsonProperty( "licenses" )]
    public string[] Licenses { get; set; } = Array.Empty<string>();

    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (LicensingConfiguration) configurationFile;
        this.LastEvaluationStartDate = source.LastEvaluationStartDate;
        this.Licenses = (string[]) source.Licenses.Clone();
    }
}