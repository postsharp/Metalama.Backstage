// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class ListCommand : CommandBase
    {
        // TODO: Description?
        public ListCommand()
            : base( "list" )
        {
            this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
        }

        private int Execute( bool verbose, IConsole console )
        {
            (var services, var trace) = this.CreateServices( console, verbose );

            var storage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, services, trace );

            var ordinal = 1;
            LicenseStringsOrdinalDictionary ordinals = new( services );

            foreach ( var license in storage.Licenses )
            {
                ordinals.Add( ordinal, license.Key );
                console.Out.WriteLine( $"({ordinal}) {license.Value?.Description ?? license.Key}" );
                ordinal++;
            }

            ordinals.Save();

            return 0;
        }
    }
}
