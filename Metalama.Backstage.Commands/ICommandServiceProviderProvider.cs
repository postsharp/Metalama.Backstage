// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Backstage.Commands
{
    /// <summary>
    /// Creates an instance <see cref="IServiceProvider"/> configured for a console command.
    /// </summary>
    public interface ICommandServiceProviderProvider
    {
        IServiceProvider GetServiceProvider( ConsoleWriter console, bool verbose );
    }
}