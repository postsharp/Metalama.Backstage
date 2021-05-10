// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.Threading.Tasks;
using PostSharp.Cli.Commands;
using System.CommandLine.Parsing;

namespace PostSharp.Cli
{
    internal class Program
    {
        private static Task<int> Main( string[] args )
        {
            var root = new PostSharpCommand();
            return root.InvokeAsync( args );
        }
    }
}
