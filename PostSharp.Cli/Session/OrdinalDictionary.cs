// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Cli.Session
{
    internal class OrdinalDictionary : Dictionary<int, string>
    {
        private const char _separator = '|';

        private readonly string _sessionDirectory = Path.Combine( Path.GetTempPath(), ".postsharp" );
        private readonly string _filePath;

        private readonly IFileSystem _fileSystem;
        private readonly IDateTimeProvider _time;

        public OrdinalDictionary( string name, IServiceProvider services )
        {
            this._filePath = Path.Combine( this._sessionDirectory, "cli-session-" + name + ".tmp" );
            this._fileSystem = services.GetService<IFileSystem>();
            this._time = services.GetService<IDateTimeProvider>();
        }

        public static OrdinalDictionary Load( string name, IServiceProvider services )
        {
            OrdinalDictionary instance = new( name, services );
            instance.Load();
            return instance;
        }

        public void Save()
        {
            StringBuilder data = new();

            foreach ( var item in this )
            {
                data.Append( item.Key );
                data.Append( _separator );

                var value = item.Value;

                if ( string.IsNullOrEmpty( value ) )
                {
                    continue;
                }

                if ( value.Contains( _separator ) )
                {
                    throw new InvalidOperationException( "Value contains separator." );
                }

                data.Append( value );
                data.Append( _separator );
            }

            var dataString = data.ToString().TrimEnd( _separator );

            this._fileSystem.CreateDirectory( this._sessionDirectory );
            this._fileSystem.WriteAllText( this._filePath, dataString );
        }

        private void Load()
        {
            if ( !this._fileSystem.FileExists( this._filePath ) )
            {
                return;
            }

            // TODO: Let the session expire here?

            var dataString = this._fileSystem.ReadAllText( this._filePath );

            if ( dataString == "" )
            {
                return;
            }

            var data = dataString.Split( _separator );

            if ( data.Length % 2 != 0 )
            {
                throw new InvalidOperationException( "Invalid data." );
            }

            for ( var i = 0; i < data.Length - 1; i += 2 )
            {
                if ( !int.TryParse( data[i], out var ordinal ) )
                {
                    throw new InvalidOperationException( "Invalid ordinal." );
                }

                var value = data[i + 1];

                this.Add( ordinal, value );
            }
        }
    }
}
