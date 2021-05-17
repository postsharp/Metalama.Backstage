// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PostSharp.Backstage.Extensibility;
using PostSharp.Backstage.Testing;
using PostSharp.Cli.Commands;
using PostSharp.Cli.Console;
using PostSharp.Cli.Tests.Console;
using Xunit;
using Xunit.Abstractions;

namespace PostSharp.Cli.Tests.Commands
{
    public abstract class CommandsTestsBase : TestsBase, ICommandServiceProvider
    {
        private readonly PostSharpCommand _rootCommand;
        private ILogger _logger;
        private TestConsole _console;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected CommandsTestsBase( ITestOutputHelper logger )
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            : base( logger )
        {
            this._rootCommand = new( this );
        }

        protected override IServiceCollection SetUpServices( IServiceCollection serviceCollection )
        {
            this._logger = this.LoggerFactory.CreateLogger( "test" );
            this._console = new( this._logger );
            serviceCollection.AddSingleton<IDiagnosticsSink>( new ConsoleDiagnosticsSink( this._console ) );
            return base.SetUpServices( serviceCollection );
        }

        protected async Task TestCommandAsync( string commandLine, string expectedOutput, string expectedError = "", int expectedExitCode = 0 )
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
