using System;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly ILogger _logger;
        private readonly TestConsole _console;

        protected CommandsTestsBase( ITestOutputHelper logger, Action<BackstageServiceCollection>? serviceBuilder = null )
            : base(
                logger,
                serviceCollection =>
                {
                    serviceCollection
                        .AddSingleton<IConsole>( services => new TestConsole( services.ToServiceProvider() ) )
                        .AddSingleton<IDiagnosticsSink>(
                            services =>
                                new ConsoleDiagnosticsSink( services.ToServiceProvider() ) );

                    serviceBuilder?.Invoke( serviceCollection );
                } )
        {
            _rootCommand = new PostSharpCommand( this );
            _logger = Services.GetOptionalTraceLogger<CommandsTestsBase>()!;
            _console = (TestConsole)Services.GetRequiredService<IConsole>();
        }

        protected async Task TestCommandAsync(
            string commandLine,
            string expectedOutput,
            string expectedError = "",
            int expectedExitCode = 0 )
        {
            _logger.LogTrace( $" < {commandLine}" );
            var exitCode = await _rootCommand.InvokeAsync( commandLine, _console );
            Assert.Equal( expectedOutput, _console.Out.ToString() );
            Assert.Equal( expectedError, _console.Error.ToString() );
            Assert.Equal( expectedExitCode, exitCode );
            _console.Clear();
            Log.Clear();
        }

        IServiceProvider ICommandServiceProvider.CreateServiceProvider( IConsole console, bool addTrace )
        {
            return Services;
        }
    }
}