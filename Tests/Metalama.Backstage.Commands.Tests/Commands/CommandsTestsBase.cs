// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Commands;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands
{
    public abstract class CommandsTestsBase : TestsBase, ICommandServiceProviderProvider
    {
        private readonly ILogger _logger;

        protected CommandsTestsBase( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base( logger )
        {
            this._logger = this.ServiceProvider.GetLoggerFactory().GetLogger( "Console" );
        }

        protected Task TestCommandAsync(
            string commandLine,
            string expectedOutput,
            string expectedError = "",
            int expectedExitCode = 0 )
            => this.TestCommandAsync( commandLine.Split( ' ' ), expectedOutput, expectedError, expectedExitCode );

        protected async Task TestCommandAsync(
            string[] commandLine,
            string expectedOutput,
            string expectedError = "",
            int expectedExitCode = 0 )
        {
            var standardOutput = new StringWriter();
            var errorOutput = new StringWriter();

            this._logger.Trace?.Log( $" < {string.Join( " ", commandLine )}" );
            var commandApp = new CommandApp();
            BackstageCommandFactory.ConfigureCommandApp( commandApp, new BackstageCommandOptions( this, standardOutput, errorOutput, AnsiSupport.No ) );
            var exitCode = await commandApp.RunAsync( commandLine );
            Assert.Equal( expectedOutput, standardOutput.ToString() );
            Assert.Equal( expectedError, errorOutput.ToString() );
            Assert.Equal( expectedExitCode, exitCode );
            this.Log.Clear();
        }

        public IServiceProvider GetServiceProvider( ConsoleWriter console, bool verbose ) => this.ServiceProvider;
    }
}