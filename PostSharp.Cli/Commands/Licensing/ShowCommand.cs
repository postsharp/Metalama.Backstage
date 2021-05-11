// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace PostSharp.Cli.Commands.Licensing
{
    internal class ShowCommand : CommandBase
    {
        // TODO: Description?
        public ShowCommand( IServicesFactory servicesFactory )
            : base( servicesFactory, "show" )
        {
            this.AddArgument( new Argument<int>( "ordinal", "the ordinal obtained by the 'postsharp license list' command" ) );

            this.Handler = CommandHandler.Create<int, bool, IConsole>( this.Execute );
        }

        private int Execute( int ordinal, bool verbose, IConsole console )
        {
            (var services, _) = this.ServicesFactory.Create( console, verbose );

            var ordinals = LicenseStringsOrdinalDictionary.Load( services );

            if ( !ordinals.TryGetValue( ordinal, out var license ) )
            {
                console.Error.WriteLine( "Unknown ordinal." );
                return 1;
            }

            console.Out.WriteLine( license );
            return 0;
        }
    }
}
