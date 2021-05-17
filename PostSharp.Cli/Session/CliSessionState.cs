// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Cli.Session
{
    internal class CliSessionState
    {
        private readonly string _sessionDirectory;
        private readonly string _filePath;

        private readonly SortedDictionary<int, string> _data = new();

        private readonly IFileSystem _fileSystem;

        public int Count => this._data.Count;

        public CliSessionState( string name, IServiceProvider services )
        {
            var standardDirectories = services.GetRequiredService<IStandardDirectories>();
            this._sessionDirectory = Path.Combine( standardDirectories.TempDirectory, "CliSessions" );
            this._filePath = Path.Combine( this._sessionDirectory, "cli-session-" + name + ".tmp" );
            this._fileSystem = services.GetRequiredService<IFileSystem>();
        }

        public void Add( int ordinal, string value )
        {
            this._data.Add( ordinal, value );
        }

        public bool TryGetValue( int ordinal, [MaybeNullWhen( returnValue: false )] out string? value )
        {
            return this._data.TryGetValue( ordinal, out value );
        }

        public void Save()
        {
            // System JSON serializer doesn't support Dictionary<int, string>
            var dataString = JsonSerializer.Serialize( this._data.ToDictionary( d => d.Key.ToString(), d => d.Value ) );

            this._fileSystem.CreateDirectory( this._sessionDirectory );
            this._fileSystem.WriteAllText( this._filePath, dataString );
        }

        public CliSessionState Load()
        {
            if ( !this._fileSystem.FileExists( this._filePath ) )
            {
                return this;
            }

            // TODO: Let the session expire here?

            var dataString = this._fileSystem.ReadAllText( this._filePath );

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>( dataString );

            if ( data != null )
            {
                foreach ( var item in data )
                {
                    this.Add( int.Parse( item.Key ), item.Value );
                }
            }

            return this;
        }
    }
}
