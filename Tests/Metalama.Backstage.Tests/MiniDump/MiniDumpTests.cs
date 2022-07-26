// Copyright (c) SharpCrafters s.r.o. All rights reserved. This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Tests.ConfigurationManager;
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
        {
            builder.AddService( typeof(IConfigurationManager), new TestConfigurationManager( builder.ServiceProvider ) );
            builder.AddService( typeof(IApplicationInfoProvider), new ApplicationInfoProvider( new TestApplicationInfo() ) );
        } ) { }

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