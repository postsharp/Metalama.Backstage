// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Application;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Maintenance;
using Metalama.Backstage.Testing;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;
using LoggerFactory = Metalama.Backstage.Diagnostics.LoggerFactory;

namespace Metalama.Backstage.Tests.Diagnostics;

public class LoggerFactoryTests : TestsBase
{
    public LoggerFactoryTests( ITestOutputHelper logger ) : base( logger ) { }

    protected override void ConfigureServices( ServiceProviderBuilder services )
    {
        services.AddSingleton<IApplicationInfoProvider>( new ApplicationInfoProvider( new TestApplicationInfo() ) );
        services.AddSingleton<ITempFileManager>( serviceProvider => new TempFileManager( serviceProvider ) );
    }

    private LoggerManager CreateLoggerManager()
        => new(
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
            ( manager, scope ) => new LoggerFactory( manager, scope ) );

    [Fact]
    public void TestSequentialWrite()
    {
        this.Time.Set( new DateTime( 1978, 6, 16 ) );

        var loggerManager = this.CreateLoggerManager();

        var loggerFactory = (LoggerFactory) loggerManager.GetLoggerFactory( Guid.NewGuid().ToString() );

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

    [Fact]
    public void TestScope()
    {
        var loggerManager = this.CreateLoggerManager();
        var loggerFactory1 = (LoggerFactory) loggerManager.GetLoggerFactory( Guid.NewGuid().ToString() );
        var loggerFactory2 = (LoggerFactory) loggerManager.GetLoggerFactory( Guid.NewGuid().ToString() );
        loggerFactory1.GetLogger( "Test" ).Trace?.Log( "Line 1" );
        loggerFactory2.GetLogger( "Test" ).Trace?.Log( "Line 2" );
        loggerFactory1.Dispose();
        loggerFactory2.Dispose();

        var allLines1 = File.ReadAllLines( loggerFactory1.LogFile! );
        Assert.Equal( "Line 1", allLines1.Last() );
        var allLines2 = File.ReadAllLines( loggerFactory2.LogFile! );
        Assert.Equal( "Line 2", allLines2.Last() );

        Assert.NotSame( loggerFactory1, loggerManager.GetLoggerFactory( loggerFactory1.Scope ) );
        Assert.NotSame( loggerFactory2, loggerManager.GetLoggerFactory( loggerFactory2.Scope ) );
    }
}