// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Backstage.Extensibility;
using System;
using System.CommandLine;

namespace PostSharp.Cli
{
    /// <summary>
    /// Creates an instance <see cref="IServiceProvider"/> configured for a console command.
    /// </summary>
    internal interface ICommandServiceProvider
    {
        /// <summary>
        /// Creates a service provider configured to redirect output and trace to a specific
        /// <see cref="IConsole"/>.
        /// </summary>
        /// <param name="console">The target <see cref="IConsole"/> for diagnostics and tracing.</param>
        /// <param name="addTrace">Determines whether an instance of the <see cref="ITrace"/> service
        /// must be registered. Should typically be <c>true</c> for high verbosity.</param>
        /// <returns></returns>
        IServiceProvider CreateServiceProvider( IConsole console, bool addTrace );
    }
}
