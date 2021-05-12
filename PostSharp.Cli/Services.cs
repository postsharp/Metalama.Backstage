// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using PostSharp.Backstage.Extensibility;
using PostSharp.Cli.Console;

namespace PostSharp.Cli
{
    internal class Services : ServiceProvider
    {
        public Services( IConsole console )
        {
            this.SetService<IDiagnosticsSink>( new ConsoleDiagnosticsSink( console ) );
            this.SetService<IDateTimeProvider>( new CurrentDateTimeProvider() );
            this.SetService<IFileSystem>( new FileSystem() );
        }
    }
}
