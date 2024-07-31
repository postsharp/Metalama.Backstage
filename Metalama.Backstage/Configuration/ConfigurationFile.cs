// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Newtonsoft.Json;
using System;
using System.IO;

namespace Metalama.Backstage.Configuration;

public abstract record ConfigurationFile
{
    private DateTime? _lastModified;

    [JsonIgnore]
    internal DateTime? LastModified
    {
        get => this._lastModified;
        set
        {
            if ( value != null && value.Value.Kind != DateTimeKind.Local )
            {
                throw new ArgumentOutOfRangeException( nameof(value), "A local time was expected." );
            }

            this._lastModified = value;
        }
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

    public virtual void Validate( Action<string> reportWarning ) { }
}