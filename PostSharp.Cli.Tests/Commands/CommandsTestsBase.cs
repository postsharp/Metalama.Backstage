// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.CommandLine;
using System.Threading.Tasks;
using PostSharp.Backstage.Testing;
using PostSharp.Cli.Commands;
using PostSharp.Cli.Tests.Console;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands
{
    public abstract class CommandsTestsBase : TestsBase
    {
        private readonly PostSharpCommand _rootCommand = new();

        protected CommandsTestsBase( ITestOutputHelper logger )
            : base( logger )
        {
        }

        protected async Task TestCommandAsync( string commandLine, string expectedOutput, string expectedError = "", int expectedExitCode = 0 )
        {
            TestConsole testConsole = new( this.Trace );
            this.Trace.WriteLine( commandLine );
            var exitCode = await this._rootCommand.InvokeAsync( commandLine, testConsole );
            Assert.Equal( expectedOutput, testConsole.Out.ToString() );
            Assert.Equal( expectedError, testConsole.Error.ToString() );
            Assert.Equal( expectedExitCode, exitCode );
        }
    }
}
