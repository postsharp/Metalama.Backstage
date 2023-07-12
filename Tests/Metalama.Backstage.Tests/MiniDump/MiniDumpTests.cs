// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Testing;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Tests.MiniDump;

public class MiniDumpTests : TestsBase
{
    public MiniDumpTests( ITestOutputHelper logger ) : base(
        logger,
        builder =>
            builder
                .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() ) )
                .AddSingleton<IConfigurationManager>( new InMemoryConfigurationManager( builder.ServiceProvider ) )
                .AddSingleton<ITempFileManager>( new TempFileManager( builder.ServiceProvider ) )
                .AddSingleton<IPlatformInfo>( new PlatformInfo( builder.ServiceProvider, null ) ) ) { }

    [Fact( Skip = "Required dotnet dump on the build agent." )]
    public void WhenWriteFileExists()
    {
        var serviceProvider = this.ServiceProvider;
        var dumper = new MiniDumper( serviceProvider );

        try
        {
#pragma warning disable CA2201
            throw new Exception();
#pragma warning restore CA2201
        }
        catch
        {
            var dumpFile = dumper.Write();

            this.Logger.WriteLine( $"Dump file '{dumpFile}' written." );
            Assert.NotNull( dumpFile );
            Assert.True( File.Exists( dumpFile ) );
        }
    }
}