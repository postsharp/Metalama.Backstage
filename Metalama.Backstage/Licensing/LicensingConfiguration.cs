// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using Metalama.Backstage.Configuration;
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