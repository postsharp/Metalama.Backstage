// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Licensing.Registration.Free;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace Metalama.DotNetTools.Commands.Licensing;

internal class RegisterFreeCommand : CommandBase
{
    public RegisterFreeCommand( ICommandServiceProviderProvider commandServiceProvider )
        : base( commandServiceProvider, "free", "Switches to Metalama Free" )
    {
        this.Handler = CommandHandler.Create<bool, IConsole>( this.Execute );
    }

    private int Execute( bool verbose, IConsole console )
    {
        this.CommandServices.Initialize( console, verbose );

        var registrar = new FreeLicenseRegistrar( this.CommandServices.ServiceProvider );

        if ( registrar.TryRegisterLicense() )
        {
            return 0;
        }
        else
        {
            console.Error.WriteLine( "Cannot switch to the Metalama Free. Use --verbose (-v) flag for details." );

            return 1;
        }
    }
}