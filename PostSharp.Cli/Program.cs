// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Cli.Commands;
using System.CommandLine;
using System.Threading.Tasks;

namespace PostSharp.Cli
{
    internal class Program
    {
        private static Task<int> Main( string[] args )
        {
            var servicesFactory = new CommandServiceProvider();
            var root = new PostSharpCommand( servicesFactory );

            return root.InvokeAsync( args );
        }
    }
}