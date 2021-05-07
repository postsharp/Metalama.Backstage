// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Threading.Tasks;
using PostSharp.Backstage.Extensibility;
using PostSharp.Cli.Commands.Licensing;

namespace PostSharp.Cli
{
    internal class Program : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        private Program()
        {
            this._services.Add( typeof( IDateTimeProvider ), new CurrentDateTimeProvider() );
            this._services.Add( typeof( IEnvironment ), new SystemEnvironment() );
            this._services.Add( typeof( IFileSystemService ), new FileSystemService() );
        }

        private static Task Main( string[] args )
        {
            return new Program().MainImplAsync( args );
        }

        private Task MainImplAsync( string[] args )
        {
            // TODO: Description?
            RootCommand rootCommand = new();

            rootCommand.Add( new LicenseCommand( this ) );

            return rootCommand.InvokeAsync( args );
        }

        object? IServiceProvider.GetService( Type serviceType ) => this._services[serviceType];
    }
}
