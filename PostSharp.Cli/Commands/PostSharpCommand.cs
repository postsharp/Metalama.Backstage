// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using PostSharp.Cli.Commands.Licensing;
using PostSharp.Cli.Console;

namespace PostSharp.Cli.Commands
{
    internal class PostSharpCommand : RootCommand
    {
        // TODO: Description?
        public PostSharpCommand()
        {
            this.Add( new LicenseCommand() );

            var verboseOption = new Option<bool>( "--verbose", "Set detailed verbosity level." );
            verboseOption.AddAlias( "-v" );
            this.AddGlobalOption( verboseOption );
        }
    }
}
