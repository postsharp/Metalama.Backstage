﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Tools.Config.Tests.Console;
using System;
using System.CommandLine;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands
{
    public abstract class CommandsTestsBase : TestsBase, ICommandServiceProviderProvider
    {
        private readonly TheRootCommand _theRootCommand;
        private readonly ILogger _logger;
        private readonly TestConsole _console;

        protected CommandsTestsBase( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base( logger )
        {
            this._theRootCommand = new TheRootCommand( this );
            this._logger = this.ServiceProvider.GetLoggerFactory().GetLogger( "Console" );
            this._console = new TestConsole( this.ServiceProvider );
        }

        protected async Task TestCommandAsync(
            string commandLine,
            string expectedOutput,
            string expectedError = "",
            int expectedExitCode = 0 )
        {
            this._logger.Trace?.Log( $" < {commandLine}" );
            var exitCode = await this._theRootCommand.InvokeAsync( commandLine, this._console );
            Assert.Equal( expectedOutput, this._console.Out.ToString() );
            Assert.Equal( expectedError, this._console.Error.ToString() );
            Assert.Equal( expectedExitCode, exitCode );
            this._console.Clear();
            this.Log.Clear();
        }

        void ICommandServiceProviderProvider.Initialize( IConsole console, bool verbose ) { }
    }
}