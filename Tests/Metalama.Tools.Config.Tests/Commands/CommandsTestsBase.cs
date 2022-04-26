// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.DotNetTools;
using Metalama.DotNetTools.Commands;
using Metalama.Tools.Config.Tests.Console;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.CommandLine;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Tools.Config.Tests.Commands
{
    public abstract class CommandsTestsBase : TestsBase, ICommandServiceProvider
    {
        private readonly TheRootCommand _theRootCommand;
        private readonly ILogger _logger;
        private readonly TestConsole _console;

        protected CommandsTestsBase( ITestOutputHelper logger, Action<ServiceProviderBuilder>? serviceBuilder = null )
            : base(
                logger,
                serviceCollection =>
                {
                    serviceCollection
                        .AddSingleton<IConsole>( new TestConsole( serviceCollection.ServiceProvider ) )
                        .AddConfigurationManager();

                    serviceBuilder?.Invoke( serviceCollection );
                } )
        {
            this._theRootCommand = new TheRootCommand( this );
            this._logger = this.ServiceProvider.GetLoggerFactory().GetLogger( "Console" );
            this._console = (TestConsole) this.ServiceProvider.GetRequiredService<IConsole>();
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

        IServiceProvider ICommandServiceProvider.Initialize( IConsole console, bool addTrace )
        {
            return this.ServiceProvider;
        }
    }
}