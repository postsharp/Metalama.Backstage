// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.CommandLine;

namespace Metalama.DotNetTools
{
    /// <summary>
    /// Creates an instance <see cref="IServiceProvider"/> configured for a console command.
    /// </summary>
    public interface ICommandServiceProviderProvider
    {
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Initializes the <see cref="ServiceProvider"/> property with a service provider
        /// configured to redirect output and trace to a specific <see cref="IConsole"/>.
        /// </summary>
        /// <param name="console">The target <see cref="IConsole"/> for diagnostics and tracing.</param>
        /// <param name="verbose">Determines whether logging should be configured. Should typically be <c>true</c> for high verbosity.</param>
        void Initialize( IConsole console, bool verbose );
    }
}