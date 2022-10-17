﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Testing;
using Metalama.Backstage.Testing.Services;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Backstage.Licensing.Tests.MiniDump;

public class MiniDumpTests : TestsBase
{
    public MiniDumpTests( ITestOutputHelper logger ) : base(
        logger,
        builder =>
            builder
                .AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() ) )
                .AddSingleton<IConfigurationManager>( new InMemoryConfigurationManager( builder.ServiceProvider ) ) ) { }

    [Fact( Skip = "Randomly fails on TeamCity, probably because of old version of DbgHelper." )]
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