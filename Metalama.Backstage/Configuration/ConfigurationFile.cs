// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using System;
using System.IO;

namespace PostSharp.Backstage.Configuration;

public abstract class ConfigurationFile
{
    [JsonIgnore]
    internal ConfigurationManager ConfigurationManager { get; private set; } = null!;

    [JsonIgnore]
    public DateTime? LastModified { get; internal set; }

    [JsonIgnore]
    public string FilePath { get; private set; } = null!;

    internal void Initialize( ConfigurationManager configurationManager, string filePath, DateTime? lastModified )
    {
        this.ConfigurationManager = configurationManager;
        this.FilePath = filePath;
        this.LastModified = lastModified;
    }

    public string ToJson()
    {
        // Serialize.
        var textWriter = new StringWriter();
        var serializer = JsonSerializer.Create();
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize( textWriter, this );

        return textWriter.ToString();
    }

    public abstract void CopyFrom( ConfigurationFile configurationFile );

    public ConfigurationFile Clone()
    {
        var clone = (ConfigurationFile) this.MemberwiseClone();
        clone.CopyFrom( this );

        return clone;
    }
}