// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class ListCommand : CommandBase
    {
        // TODO: Description?
        public ListCommand( IServiceProvider services )
            : base( services, "list" )
        {
            this.Handler = CommandHandler.Create<InvocationContext>( this.Execute );
        }

        private int Execute( InvocationContext context )
        {
            var storage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, this.Services, CreateTrace( context ) );

            var ordinal = 1;
            LicenseStringsOrdinalDictionary ordinals = new( this.Services );

            foreach ( var license in storage.Licenses )
            {
                ordinals.Add( ordinal, license.Key );
                context.Console.Out.WriteLine( $"({ordinal}) {license.Value?.Description ?? license.Key}" );
                ordinal++;
            }

            ordinals.Save();

            return 0;
        }
    }
}
