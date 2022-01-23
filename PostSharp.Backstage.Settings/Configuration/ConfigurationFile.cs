// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Newtonsoft.Json;
using PostSharp.Backstage.Diagnostics;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Extensibility.Extensions;
using PostSharp.Backstage.Utilities;
using System;
using System.IO;

namespace PostSharp.Backstage.Configuration;

public abstract class ConfigurationFile
{
    [JsonIgnore]
    public string? FilePath { get; private set; }

    [JsonIgnore]
    public abstract string FileName { get; }

    /// <summary>
    /// Saves the current object to the file specified in the <see cref="FilePath"/> property.
    /// </summary>
    public void Save( IServiceProvider serviceProvider )
    {
        var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        var directories = serviceProvider.GetRequiredService<IStandardDirectories>();
        var logger = serviceProvider.GetLoggerFactory().Configuration();

        // Create the directory if it does not exist.
        var directoryName = directories.ApplicationDataDirectory;

        RetryHelper.Retry(
            () =>
            {
                if ( !fileSystem.DirectoryExists( directoryName ) )
                {
                    fileSystem.CreateDirectory( directoryName );
                }
            } );

        var json = this.ToJson();
        var path = Path.Combine( directoryName, this.FileName );

        // Write to file.
        RetryHelper.Retry( () => fileSystem.WriteAllText( path, json ), logger: logger );
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

    /// <summary>
    /// Loads a configuration file from disk. If the file is not found or is invalid,
    /// a default instance is returned.
    /// </summary>
    protected static T Load<T>( IServiceProvider services )
        where T : ConfigurationFile, new()
    {
        T prototype = new();
        var fileSystem = services.GetRequiredService<IFileSystem>();
        var logger = services.GetLoggerFactory().Configuration();

        var standardDirectories = services.GetRequiredService<IStandardDirectories>();
        var path = Path.Combine( standardDirectories.ApplicationDataDirectory, prototype.FileName );

        T? configuration;

        try
        {
            if ( fileSystem.FileExists( path ) )
            {
                var serializer = JsonSerializer.Create();
                var json = RetryHelper.Retry( () => fileSystem.ReadAllText( path ), logger: logger );
                configuration = serializer.Deserialize<T>( new JsonTextReader( new StringReader( json ) ) );
            }
            else
            {
                configuration = null;
            }
        }
        catch
        {
            // We don't want to crash if we fail here.
            configuration = null;
        }

        configuration ??= prototype;
        configuration.FilePath = path;

        return configuration;
    }
}