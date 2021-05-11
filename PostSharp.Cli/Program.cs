// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using PostSharp.Cli.Commands;

namespace PostSharp.Cli
{
    internal class Program
    {
        private static Task<int> Main( string[] args )
        {
            var servicesFactory = new ServicesFactory();
            var root = new PostSharpCommand( servicesFactory );
            return root.InvokeAsync( args );
        }
    }
}
