// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.DotNetTools;
using Metalama.Tools.Config.Tests.Console;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Metalama.Tools.Config.Tests;

public class CommandServiceProviderTests
{
    [Fact]
    public void Initialize()
    {
        // This is to test that CommandServiceProvider initialization is correct because this class is skipped in other tests.

        var serviceCollection = new ServiceCollection();
        var instance = new CommandServiceProvider();
        instance.Initialize( new TestConsole( serviceCollection.BuildServiceProvider() ), false );
    }
}