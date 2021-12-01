// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.Extensions.DependencyInjection;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Testing;
using PostSharp.Cli.Commands;
using PostSharp.Cli.Console;
using PostSharp.Cli.Tests.Console;
using System;
using System.CommandLine;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands
{
    public abstract class CommandsTestsBase : TestsBase, ICommandServiceProvider
    {
        private readonly PostSharpCommand _rootCommand;
        private readonly ILogger _logger;
        private readonly TestConsole _console;

        protected CommandsTestsBase( ITestOutputHelper logger, Action<BackstageServiceCollection>? serviceBuilder = null )
            : base(
                logger,
                serviceCollection =>
                {
                    serviceCollection
                        .AddSingleton<IConsole>( services => new TestConsole( services.ToServiceProvider() ) )
                        .AddSingleton<IBackstageDiagnosticSink>(
                            services =>
                                new ConsoleDiagnosticsSink( services.ToServiceProvider() ) );

                    serviceBuilder?.Invoke( serviceCollection );
                } )
        {
            this._rootCommand = new PostSharpCommand( this );
            this._logger = this.Services.GetOptionalTraceLogger<CommandsTestsBase>()!;
            this._console = (TestConsole) this.Services.GetRequiredService<IConsole>();
        }

        protected async Task TestCommandAsync(
            string commandLine,
            string expectedOutput,
            string expectedError = "",
            int expectedExitCode = 0 )
        {
            this._logger.LogTrace( $" < {commandLine}" );
            var exitCode = await this._rootCommand.InvokeAsync( commandLine, this._console );
            Assert.Equal( expectedOutput, this._console.Out.ToString() );
            Assert.Equal( expectedError, this._console.Error.ToString() );
            Assert.Equal( expectedExitCode, exitCode );
            this._console.Clear();
            this.Log.Clear();
        }

        IServiceProvider ICommandServiceProvider.CreateServiceProvider( IConsole console, bool addTrace )
        {
            return this.Services;
        }
    }
}