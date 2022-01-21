// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Cli.Commands.Licensing;
using PostSharp.Cli.Commands.Logging;
using System.CommandLine;

namespace PostSharp.Cli.Commands
{
    internal class TheRootCommand : RootCommand
    {
        public TheRootCommand( ICommandServiceProvider commandServiceProvider )
            : base( "Management tool for PostSharp" )
        {
            this.Add( new LicenseCommand( commandServiceProvider ) );
            this.Add( new LoggingCommand( commandServiceProvider ) );

            var verboseOption = new Option<bool>( "--verbose", "Set detailed verbosity level" );
            verboseOption.AddAlias( "-v" );
            this.AddGlobalOption( verboseOption );
        }
    }
}