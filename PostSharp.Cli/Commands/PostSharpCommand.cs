// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using PostSharp.Cli.Commands.Licensing;

namespace PostSharp.Cli.Commands
{
    internal class PostSharpCommand : RootCommand
    {
        public PostSharpCommand( IServicesFactory servicesFactory )
            : base( "Management tool for PostSharp" )
        {
            this.Add( new LicenseCommand( servicesFactory ) );

            var verboseOption = new Option<bool>( "--verbose", "Set detailed verbosity level" );
            verboseOption.AddAlias( "-v" );
            this.AddGlobalOption( verboseOption );
        }
    }
}
