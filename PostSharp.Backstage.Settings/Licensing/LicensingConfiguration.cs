// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using PostSharp.Backstage.Configuration;
using System;

namespace PostSharp.Backstage.Licensing;

public class LicensingConfiguration : ConfigurationFile
{
    /// <summary>
    /// Gets or sets the date of the last evaluation period.
    /// </summary>
    [JsonProperty( "lastEvaluationStartDate" )]
    public DateTime? LastEvaluationStartDate { get; set; }

    [JsonProperty( "licenses" )]
    public string[] Licenses { get; set; } = Array.Empty<string>();

    public static LicensingConfiguration Load( IServiceProvider services ) => Load<LicensingConfiguration>( services );

    public override string FileName => "licensing.json";
}