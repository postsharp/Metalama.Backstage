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

        protected CommandsTestsBase( ITestOutputHelper logger )
            : base( logger )
        {
            this._logger = this.ServiceProvider.GetLoggerFactory().GetLogger( "Console" );
        }

        protected Task TestCommandAsync(
            string commandLine,
            string? expectedOutput = null,
            string? expectedError = null,
            int expectedExitCode = 0 )
            => this.TestCommandAsync( commandLine.Split( ' ' ), expectedOutput, expectedError, expectedExitCode );

        protected async Task TestCommandAsync(
            string[] commandLine,
            string? expectedOutput = null,
            string? expectedError = null,
            int expectedExitCode = 0 )
        {
            var standardOutput = new StringWriter();
            var errorOutput = new StringWriter();

            this._logger.Trace?.Log( $">> {string.Join( " ", commandLine )}" );

            var commandApp = new CommandApp();
            BackstageCommandFactory.ConfigureCommandApp( commandApp, new BackstageCommandOptions( this, standardOutput, errorOutput, AnsiSupport.No ) );
            var exitCode = await commandApp.RunAsync( commandLine );

            this._logger.Trace?.Log( standardOutput.ToString() );
            this._logger.Trace?.Log( errorOutput.ToString() );

            if ( expectedOutput != null )
            {
                Assert.Contains( expectedOutput, standardOutput.ToString(), StringComparison.OrdinalIgnoreCase );
            }

            if ( expectedError != null )
            {
                Assert.Contains( expectedError, errorOutput.ToString(), StringComparison.OrdinalIgnoreCase );
            }

            Assert.Equal( expectedExitCode, exitCode );
            this.Log.Clear();
        }

        IServiceProvider ICommandServiceProviderProvider.GetServiceProvider( CommandServiceProviderArgs args ) => this.ServiceProvider;
    }
}