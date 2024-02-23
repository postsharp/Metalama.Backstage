// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Testing;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using LoggerFactory = Metalama.Backstage.Diagnostics.LoggerFactory;

namespace Metalama.Backstage.Tests.Diagnostics;

public class LoggerFactoryTests : TestsBase
{
    public LoggerFactoryTests( ITestOutputHelper logger ) : base(
        logger,
        builder =>
        {
            builder.AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() ) );
            builder.AddSingleton<ITempFileManager>( new TempFileManager( builder.ServiceProvider ) );
        } ) { }

    [Fact]
    public void Test()
    {
        this.Time.Set( new DateTime( 1978, 6, 16 ) );

        var loggerFactory = new LoggerFactory(
            this.ServiceProvider,
            new DiagnosticsConfiguration()
            {
                Logging = new LoggingConfiguration()
                {
                    TraceCategories = ImmutableDictionary<string, bool>.Empty.Add( "*", true ),
                    Processes = ImmutableDictionary<string, bool>.Empty.Add( ProcessKind.Other.ToString(), true )
                }
            },
            ProcessKind.Other,
            Guid.NewGuid().ToString() );

        var logger = loggerFactory.GetLogger( "Test" );

        var n = 10000;

        for ( var i = 0; i < n; i++ )
        {
            logger.Trace?.Log( i.ToString( CultureInfo.InvariantCulture ) );

            if ( i % 100 == 0 )
            {
                // This is to cause many tasks to start.
                Thread.Sleep( 10 );
            }
        }

        loggerFactory.Dispose();

        var allLines = File.ReadAllLines( loggerFactory.LogFile! );

        Assert.Equal( n, allLines.Length );
    }
}