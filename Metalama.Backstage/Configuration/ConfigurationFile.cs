// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;
using System;
using System.IO;

namespace Metalama.Backstage.Configuration;

public abstract record ConfigurationFile
{
    [JsonIgnore]
    public DateTime? LastModified { get; init; }

    public string ToJson()
    {
        // Serialize.
        var textWriter = new StringWriter();
        var serializer = JsonSerializer.Create();
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize( textWriter, this );

        return textWriter.ToString();
    }
}