// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Text;
using PostSharp.Backstage.Extensibility;

namespace PostSharp.Cli.Session
{
    public class OrdinalDictionary : Dictionary<int, string>
    {
        private const char _separator = '|';

        private readonly string _environmentVariableName;

        private readonly IEnvironment _environment;

        public OrdinalDictionary( string name, IServiceProvider services )
        {
            this._environmentVariableName = "POSTSHARP_CLI_SESSION_" + name;
            this._environment = services.GetService<IEnvironment>();
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
            this._environment.SetEnvironmentVariable( this._environmentVariableName, dataString );
        }

        private void Load()
        {
            if ( !this._environment.TryGetEnvironmentVariable( this._environmentVariableName, out var dataString ) )
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
