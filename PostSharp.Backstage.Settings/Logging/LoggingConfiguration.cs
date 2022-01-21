// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using PostSharp.Backstage.Configuration;
using System;
using System.Collections.Generic;

namespace PostSharp.Backstage.Logging;

public class LoggingConfiguration : ConfigurationFile
{
    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled at all.
    /// </summary>
    [JsonProperty( "processes" )]
    public Dictionary<LoggingProcessKind, bool> Processes { get; set; }


    /// <summary>
    /// Gets or sets the list of categories that are enabled for trace-level logging.
    /// </summary>
    [JsonProperty( "categories" )]
    public Dictionary<string, bool> Categories { get; set; }

    public LoggingConfiguration()
    {
        // This is to make the null analysis happy.
        this.Processes = null!;
        this.Categories = null!;

        this.Reset();
    }

    public void Reset()
    {
        this.Processes = new Dictionary<LoggingProcessKind, bool>
        {
            [LoggingProcessKind.Compiler] = false,
            [LoggingProcessKind.Rider] = false,
            [LoggingProcessKind.DevEnv] = false,
            [LoggingProcessKind.RoslynCodeAnalysisService] = false
        };

        this.Categories = new Dictionary<string, bool>( StringComparer.OrdinalIgnoreCase )
        {
            ["*"] = false, ["Licensing"] = false
        };
    }

    public static LoggingConfiguration Load( IServiceProvider services ) =>
        Load<LoggingConfiguration>( services );

    public override string FileName => "logging.json";
}