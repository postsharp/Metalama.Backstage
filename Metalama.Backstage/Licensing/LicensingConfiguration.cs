﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

    // Originally, the license configuration allowed for multiple license keys. We keep the array for backward compatility,
    // but we no longer store more than one key there.
    [JsonProperty( "licenses" )]
    public string[] Licenses { get; set; } = Array.Empty<string>();

    public override void CopyFrom( ConfigurationFile configurationFile )
    {
        var source = (LicensingConfiguration) configurationFile;
        this.LastEvaluationStartDate = source.LastEvaluationStartDate;
        this.Licenses = (string[]) source.Licenses.Clone();
    }
}