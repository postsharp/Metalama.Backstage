// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.DotNetTools.Commands.Licensing;
using Metalama.DotNetTools.Commands.Logging;
using System.CommandLine;

namespace Metalama.DotNetTools.Commands
{
    internal class TheRootCommand : RootCommand
    {
        public TheRootCommand( ICommandServiceProvider commandServiceProvider )
            : base( "Management tool for PostSharp" )
        {
            this.Add( new LicenseCommand( commandServiceProvider ) );
            this.Add( new DiagnosticsCommand( commandServiceProvider ) );

            var verboseOption = new Option<bool>( "--verbose", "Set detailed verbosity level" );
            verboseOption.AddAlias( "-v" );
            this.AddGlobalOption( verboseOption );
        }
    }
}