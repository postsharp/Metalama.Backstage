// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine.Invocation;
using System.Text;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Licensing.Registration;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class ListCommand : CommandBase
    {
        // TODO: Description?
        public ListCommand( IServiceProvider services, ITrace trace )
            : base( services, trace, "license" )
        {
            this.Handler = CommandHandler.Create( Execute );
        }

        private int Execute()
        {
            var storage = LicenseFileStorage.OpenOrCreate( StandardLicenseFilesLocations.UserLicenseFile, this.Services, this.Trace );

            int ordinal = 1;

            foreach ( var license in storage.Licenses )
            {

            }

            return 0;
        }
    }
}
